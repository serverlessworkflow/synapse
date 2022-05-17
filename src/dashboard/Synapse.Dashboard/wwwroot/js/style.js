window.getComputedStyleProperty = (propertyName) => {
    let styles = window.getComputedStyle(document.documentElement);
    return styles.getPropertyValue(propertyName);
}