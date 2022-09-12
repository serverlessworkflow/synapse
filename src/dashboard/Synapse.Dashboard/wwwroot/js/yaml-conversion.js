export function yamlToJson(yamlText) {
    if (!yamlText) return "";
    var obj = jsyaml.load(yamlText);
    return JSON.stringify(obj, null, 2);
}

export function jsonToYaml(jsonText) {
    if (!jsonText) return "";
    var obj = JSON.parse(jsonText);
    return jsyaml.dump(obj);
}