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