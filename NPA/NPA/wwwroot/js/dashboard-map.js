// Google GeoChart for NPA Dashboard
let _geoChartLoaded = false;

function ensureGeoChart() {
    return new Promise((resolve) => {
        if (_geoChartLoaded) { resolve(); return; }
        google.charts.load('current', { packages: ['geochart'] });
        google.charts.setOnLoadCallback(() => { _geoChartLoaded = true; resolve(); });
    });
}

window.drawProjectGeoChart = async function (countryData) {
    await ensureGeoChart();

    var data = new google.visualization.DataTable();
    data.addColumn('string', 'Country');
    data.addColumn('number', 'Projects');
    data.addRows(countryData);

    var options = {
        colorAxis: { colors: ['#fde8c8', '#e8a843', '#C8811A', '#8B5E00'] },
        backgroundColor: 'transparent',
        datalessRegionColor: '#eeeeee',
        defaultColor: '#eeeeee',
        legend: { textStyle: { color: '#333', fontSize: 12 } },
        tooltip: { isHtml: true, trigger: 'focus' },
        keepAspectRatio: true
    };

    var container = document.getElementById('project-geochart');
    if (container) {
        var chart = new google.visualization.GeoChart(container);
        chart.draw(data, options);
    }
};
