(() => {
    if (dagre == null) {
        throw 'dagre needs to be loaded first';
    }
    window.dagre                  = dagre;
    window.neuroglia              = window.neuroglia || {};
    window.neuroglia.blazor       = window.neuroglia.blazor || {};
    window.neuroglia.blazor.dagre = window.neuroglia.blazor.dagre || {};
    window.neuroglia.blazor.dagre.layout = (graph) => {
        dagre.layout(graph);
        return graph;
    };
    window.neuroglia.blazor.dagre.graph = (options) => {
        return new dagre.graphlib.Graph(options).setDefaultEdgeLabel(() => ({ }));
    };
    window.neuroglia.blazor.dagre.write = (graph) => {
        return JSON.stringify(dagre.graphlib.json.write(graph));
    };
    window.neuroglia.blazor.dagre.read = (str) => {
        return dagre.graphlib.json.read(JSON.parse(str));
    };
    window.neuroglia.blazor.preventScroll = (graphElement) => {
        graphElement.addEventListener("wheel", e => e.preventDefault(), { passive: false });
    }
    window.neuroglia.blazor.getCenter = (graphElement) => {
        const svgBbox = graphElement.getBoundingClientRect();
        const graphBbox = graphElement.getBBox();
        return {
            x: (svgBbox.width - graphBbox.width) / 2,
            y: (svgBbox.height - graphBbox.height) / 2
        };
    }
    window.neuroglia.blazor.getScale = (graphElement) => {
        const svgBbox = graphElement.getBoundingClientRect();
        const graphBbox = graphElement.querySelector('g.graph').getBBox();
        const wScale = Math.floor(svgBbox.width) / Math.ceil(graphBbox.width);
        const hScale = Math.floor(svgBbox.height) / Math.ceil(graphBbox.height);
        if (graphBbox.width / graphBbox.height >= 1) {
            if (graphBbox.height * wScale < svgBbox.height) {
                return wScale;
            }
            return hScale;
        }
        else if (graphBbox.width * hScale < svgBbox.width) {
            return hScale;
        }
        return wScale;
    }
})();