console.log("price-chart.js loaded");

document.addEventListener("DOMContentLoaded", () => {

    const element = document.getElementById("price-data");

    if (!element) {
        return;
    }

    const prices = JSON.parse(element.textContent);

    const labels = prices.map(p =>
        new Date(p.startTime).toLocaleTimeString(
            "da-DK",
            {
                hour: "2-digit",
                minute: "2-digit"
            }
        )
    );

    const values = prices.map(p => p.pricePerKwh);
    const ctx = document.getElementById("priceChart");

    new Chart(ctx, {
        type: "line",

        data: {
            labels: labels,
            datasets: [
                {
                    label: "Pris kr./kWh",
                    data: values,
                    tension: 0.2,
                    pointRadius: 2
                }
            ]
        },
        options: {
            responsive: true,
            plugins: {
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return context.parsed.y.toFixed(2).replace(".", ",") + " kr./kWh";
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function (value) {
                            return value.toFixed(2).replace(".", ",") + " kr.";
                        }
                    }
                }
            }
        }
    });
});
