const getTranslate = (node, axis) => {
    const style = window.getComputedStyle(node);
    const matrix = new WebKitCSSMatrix(style.transform);
    switch (axis) {
        case 'x':
            return matrix.m41;
        case 'y':
            return matrix.m42;
        case 'z':
            return matrix.m43;
        default:
            throw `unknown axis '${axis}`;
    }
};

class NodeGhost {
    constructor(node) {
        this.node = node;
        this.ghost = node.cloneNode(true);
        this.ghost.classList.add('ghost');
        this.transformable = this.ghost.querySelector('g[transform]');
        this.node.parentNode.appendChild(this.ghost);
    }
    dispose() {
        this.node.parentNode.removeChild(this.ghost);
    }
    move(x, y) {
        const translateX = getTranslate(this.transformable, 'x') + x;
        const translateY = getTranslate(this.transformable, 'y') + y;
        this.transformable.setAttribute('transform', `translate(${translateX}, ${translateY})`);
    }
}

export const createNodeGhost = (node) => new NodeGhost(node);