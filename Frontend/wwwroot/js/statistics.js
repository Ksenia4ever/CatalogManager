(() => {
    "use strict";

    const status = document.getElementById("statisticsStatus");
    const refresh = document.getElementById("statisticsRefresh");
    const charts = {};
    const palette = ["#2563eb", "#0d9488", "#f59e0b", "#8b5cf6", "#ef4444", "#06b6d4", "#84cc16"];

    const setText = (id, value) => {
        document.getElementById(id).textContent = String(value);
    };

    function drawChart(key, elementId, type, rows, title) {
        if (charts[key]) charts[key].destroy();
        const labels = rows.map(item => item.label);
        const counts = rows.map(item => item.count);
        charts[key] = new Chart(document.getElementById(elementId), {
            type,
            data: {
                labels,
                datasets: [{
                    label: title,
                    data: counts,
                    backgroundColor: labels.map((_, index) => palette[index % palette.length]),
                    borderRadius: type === "bar" ? 6 : undefined,
                    borderWidth: 0
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: type === "doughnut", position: "bottom" },
                    tooltip: { enabled: true }
                },
                scales: type === "bar" ? {
                    y: { beginAtZero: true, ticks: { precision: 0 } }
                } : {}
            }
        });
    }

    async function loadStatistics() {
        refresh.disabled = true;
        status.hidden = false;
        status.className = "alert alert-info";
        status.textContent = "Loading statistics...";
        try {
            const response = await fetch("/Statistics/Data", { cache: "no-store" });
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            const data = await response.json();
            if (!Array.isArray(data.byCategory) || !Array.isArray(data.byStore) || !Array.isArray(data.byPriceRange)) {
                throw new Error("Unexpected statistics response");
            }
            if (typeof Chart === "undefined") throw new Error("Chart.js did not load. Check your internet connection.");

            setText("totalProducts", data.totalProducts);
            setText("totalCategories", data.totalCategories);
            setText("totalStores", data.totalStores);

            drawChart("categories", "categoryChart", "bar", data.byCategory, "Products");
            drawChart("stores", "storeChart", "doughnut", data.byStore, "Products");
            drawChart("prices", "priceChart", "bar", data.byPriceRange, "Products");
            status.className = "alert alert-success";
            status.textContent = data.totalProducts === 0
                ? "No products yet. Add products to see data in the charts."
                : "Statistics updated successfully.";
        } catch (error) {
            status.className = "alert alert-danger";
            status.textContent = `Could not load statistics: ${error.message}`;
        } finally {
            refresh.disabled = false;
        }
    }

    refresh.addEventListener("click", loadStatistics);
    loadStatistics();
})();