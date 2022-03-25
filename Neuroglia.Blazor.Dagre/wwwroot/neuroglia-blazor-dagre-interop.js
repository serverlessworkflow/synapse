(() => {
    if (dagre == null) {
        throw 'dagre needs to be loaded first';
    }
    window.dagre                  = dagre;
    window.neuroglia              = window.neuroglia || {};
    window.neuroglia.blazor       = window.neuroglia.blazor || {};
    window.neuroglia.blazor.dagre = window.neuroglia.blazor.dagre || {};
    window.neuroglia.blazor.dagre.layout = (g) => {
        dagre.layout(g);
        return graph;
    };
    window.neuroglia.blazor.dagre.graph = (options) => {
        return new dagre.graphlib.Graph(options);
    };
})();