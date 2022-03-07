function enableJsonValidation08(modelUri) {
    monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
        validate: true,
        allowComments: false,
        enableSchemaRequest: true,
        schemas: [{
            uri: `${window.location.protocol}//${window.location.host}/schemas/0.8/workflow.json`,
            fileMatch: [modelUri.toString()]
        }]
    });
}

function getModelMarkers(owner) {
    return monaco.editor.getModelMarkers({ owner }) || [];
}