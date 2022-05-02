let charts = { };
window.renderChart = (id, config) => {
    let ctx = document.getElementById(id).getContext('2d');
    let chart = charts[id];
    if (chart) {
        chart.destroy();
    }
    chart = new Chart(ctx, config);
    charts[id] = chart;
}