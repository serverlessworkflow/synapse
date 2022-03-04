document.addEventListener('DOMContentLoaded', () => {
    const $animationContainer = document.getElementById('animation-container');
    const now = new Date();
    const localStorageKey = 'next-logo-animation';
    const nextLogoAnimationDateStr = window.localStorage.getItem(localStorageKey);
    const nextLogoAnimationDate = new Date(parseInt(nextLogoAnimationDateStr, 10));
    if (nextLogoAnimationDate && now < nextLogoAnimationDate) {
        $animationContainer.remove();
    }
    else {
        const removeContainer = (e) => {
            if ($animationContainer.isSameNode(e.target)) {
                $animationContainer.removeEventListener('animationend', removeContainer);
                $animationContainer.remove();
                const nextAppearance = new Date();
                nextAppearance.setHours(0);
                nextAppearance.setMinutes(0);
                nextAppearance.setSeconds(0);
                nextAppearance.setDate(nextAppearance.getDate() + 1);
                window.localStorage.setItem(localStorageKey, nextAppearance.getTime());
            }
        };
        $animationContainer.addEventListener('animationend', removeContainer);
    }
});