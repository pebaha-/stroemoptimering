function renderPriceChart(container) {
    const priceElement = container.querySelector(".price-data");

    if (!priceElement) {
        return;
    }

    const prices = JSON.parse(priceElement.textContent);
    const optimalElement = container.querySelector(".optimal-period-data");
    const optimalPeriod = optimalElement
        ? JSON.parse(optimalElement.textContent)
        : null;
    const ctx = container.querySelector(".price-chart");

    if (!ctx) {
        return;
    }

    const labels = prices.map(price =>
        new Date(price.startTime).toLocaleTimeString("da-DK", {
            hour: "2-digit",
            minute: "2-digit"
        }));
    const values = prices.map(price => price.pricePerKwh);
    const optimalValues = prices.map(price => {
        if (!optimalPeriod) {
            return null;
        }

        const time = new Date(price.startTime);
        const start = new Date(optimalPeriod.startTime);
        const end = new Date(optimalPeriod.endTime);

        return time >= start && time < end ? price.pricePerKwh : null;
    });

    new Chart(ctx, {
        type: "line",
        data: {
            labels,
            datasets: [
                {
                    label: "Pris kr./kWh",
                    data: values,
                    tension: 0.2,
                    borderColor: "rgb(54, 162, 235)",
                    backgroundColor: "rgba(54, 162, 235, 0.1)",
                    borderWidth: 2,
                    pointRadius: 2,
                    pointHoverRadius: 20
                },
                {
                    label: "Billigste periode",
                    data: optimalValues,
                    tension: 0.2,
                    borderColor: "rgb(25, 135, 84)",
                    backgroundColor: "rgba(25, 135, 84, 0.15)",
                    borderWidth: 4,
                    pointRadius: 5,
                    pointHoverRadius: 7,
                    spanGaps: false
                }
            ]
        },
        options: {
            responsive: true,
            plugins: {
                tooltip: {
                    callbacks: {
                        label(context) {
                            return `${context.parsed.y.toFixed(2).replace(".", ",")} kr./kWh`;
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback(value) {
                            return `${value.toFixed(2).replace(".", ",")} kr.`;
                        }
                    }
                }
            }
        }
    });
}

document.addEventListener("DOMContentLoaded", () => {
    const container = document.getElementById("price-results");

    if (container) {
        renderPriceChart(container);
    }
});

document.body.addEventListener("htmx:afterSwap", event => {
    if (event.detail.target.id !== "price-results") {
        return;
    }

    const container = document.getElementById("price-results");

    if (container) {
        renderPriceChart(container);
    }
});
