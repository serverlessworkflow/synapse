# Monaco Editor - Prebuilt YAML support
A prebuilt version of [monaco-yaml](https://github.com/remcohaszing/monaco-yaml) available straight out in the browser.

# Usage
You can use it straight out of the box, just load the precompiled `monaco-editor.js` in your browser.

```
npm install monaco-yaml-prebuilt
```

```html
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="utf-8" />
    <title>Monaco YAML prebuilt</title>
  </head>
  <body>
    <div style="display: flex;">
      <div id="yaml-editor" style="width: 95vw; height: 95vh;"></div>
    </div>
    <script src="./node_modules/monaco-yaml-prebuilt/dist/monaco-editor.js"></script>
    <script src="./index.js"></script>
  </body>
</html>

```

Then two objects will be available in your global `window` scope, `monaco` for [monaco-editor](https://github.com/microsoft/monaco-editor) and `monacoYaml` for [monaco-yaml](https://github.com/remcohaszing/monaco-yaml) specific APIs:
```javascript
const yamlModelUri = monaco.Uri.parse('a://b/foo.yaml');

const diagnosticsOptions = {
  enableSchemaRequest: true,
  hover: true,
  completion: true,
  validate: true,
  format: true,
  schemas: [
    {
      uri: 'http://myserver/foo-schema.json',
      fileMatch: ['*'],
      schema: {
        type: 'object',
        properties: {
          p1: {
            enum: ['v1', 'v2'],
          },
          p2: {
            $ref: 'http://myserver/bar-schema.json',
          },
        },
      },
    },
    {
      uri: 'http://myserver/bar-schema.json',
      schema: {
        type: 'object',
        properties: {
          q1: {
            enum: ['x1', 'x2'],
          },
        },
      },
    },
  ],
};

// YAML specific API
monacoYaml.setDiagnosticsOptions(diagnosticsOptions);

const yaml = 'p1: \np2: \n';

monaco.editor.create(document.getElementById('yaml-editor'), {
  automaticLayout: true,
  model: monaco.editor.createModel(yaml, 'yaml', yamlModelUri),
});
```

# Changing languages support
By default, only `yaml` and `json` are supported. If you want to add more, you can edit `webpack.config.js` and rebuild the package:

```
git clone https://github.com/serverlessworkflow/monaco-yaml-prebuilt.git
cd monaco-yaml-prebuilt
... edit webpack.config.js
npm run build
```

See [Integrating the ESM version of the Monaco Editor - Option 1: Using the Monaco Editor WebPack Plugin](https://github.com/microsoft/monaco-editor/blob/main/docs/integrate-esm.md#option-1-using-the-monaco-editor-webpack-plugin) for more info.