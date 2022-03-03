document.addEventListener('DOMContentLoaded', () => {
    const $animationContainer = document.getElementById('animation-container');
    const removeContainer = (e) => {
        if ($animationContainer.isSameNode(e.target)) {
            $animationContainer.removeEventListener('animationend', removeContainer);
            $animationContainer.remove();
        }
    };
    $animationContainer.addEventListener('animationend', removeContainer);
});