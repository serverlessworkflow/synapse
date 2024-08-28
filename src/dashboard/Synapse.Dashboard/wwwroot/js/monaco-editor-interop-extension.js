import { parse } from "https://esm.sh/json-source-map";
import { LineCounter, parseDocument } from "https://esm.sh/yaml";

/**
 * Adds a validation schema to monaco editor's diagnostics options
 * @param {any} schema The validation schema to add
 * @param {any} schemaUri The schema identifier
 * @param {any} schemaType The schema type, used to match the "file"/model URI
 * @returns
 */
export function addValidationSchema(schema, schemaUri, schemaType) {
    // JSON
    if (!monaco.languages.json.jsonDefaults.diagnosticsOptions.schemas.find(s => s.uri === schemaUri)) {
        monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
            validate: true,
            allowComments: false,
            enableSchemaRequest: true,
            schemas: [
                ...monaco.languages.json.jsonDefaults.diagnosticsOptions.schemas,
                {
                    schema: JSON.parse(schema),
                    uri: schemaUri,
                    fileMatch: [schemaType]
                }
            ]
        });
    }
    // YAML
    if (!monacoYaml.yamlDefaults.diagnosticsOptions.schemas.find(s => s.uri === schemaUri)) {
        monacoYaml.setDiagnosticsOptions({
            validate: true,
            enableSchemaRequest: true,
            //hover: true,
            //completion: true,
            //format: true,
            schemas: [
                ...monacoYaml.yamlDefaults.diagnosticsOptions.schemas,
                {
                    schema: JSON.parse(schema),
                    uri: schemaUri,
                    fileMatch: [schemaType]
                }
            ]
        });
    }
}

function getJsonPointeRangeForJson(source, jsonPointer) {
    const { pointers } = parse(source);
    const node = pointers[jsonPointer];
    if (!node) {
        throw new Error(`Unable to find JSON pointer '${jsonPointer}'`);
    }
    return {
        startLineNumber: node.key.line + 1,
        startColumn: node.key.column + 1,
        endLineNumber: node.valueEnd.line + 1,
        endColumn: node.valueEnd.column + 1
    };
}
function getJsonPointeRangeForYaml(source, jsonPointer) {
    const lineCounter = new LineCounter(source);
    const document = parseDocument(source, { keepSourceTokens: true, lineCounter });
    const node = jsonPointer.split('/').slice(1).reduce((n, key) => {
        const idx = parseInt(key, 10);
        const nextNode = n.get(!isNaN(idx) ? idx : key, true);
        if (!nextNode) {
            throw new Error(`Unable to find JSON pointer '${jsonPointer}'`);
        }
        return nextNode;
    }, document);
    const start = lineCounter.linePos(node.range[0]);
    const end = lineCounter.linePos(node.range[1]);
    return {
        startLineNumber: start.line,
        startColumn: start.col,
        endLineNumber: end.line,
        endColumn: end.col
    }
}

/**
 * Finds the range in a JSON/YAML text corresponding to a provided JSON Pointer
 * @param {string} source The source JSON/YAML text
 * @param {string} jsonPointer The JSON pointer to find the range for
 * @param {'json'|'yaml'} language The language of the source, JSON or YAML
 * @returns {Monaco.Range} The corresponding range
 */
export function getJsonPointerRange(source, jsonPointer, language) {
    if (language.toLowerCase() === 'json') {
        return getJsonPointeRangeForJson(source, jsonPointer);
    }
    else if (language.toLowerCase() === 'yaml') {
        return getJsonPointeRangeForYaml(source, jsonPointer);
    }
    else {
        throw new Error(`Invalid language '${language}'`);
    }
}