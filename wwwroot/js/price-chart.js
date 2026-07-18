console.log("price-chart.js loaded");

document.addEventListener("DOMContentLoaded", () => {

    const priceElement = document.getElementById("price-data");

    if (!priceElement) {
        console.error("price-data element not found");
        return;
    }

    const prices = JSON.parse(priceElement.textContent);

    console.log("Prices:", prices);


    const optimalElement =
        document.getElementById("optimal-period-data");

    const optimalPeriod =
        optimalElement
            ? JSON.parse(optimalElement.textContent)
            : null;


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


    const optimalValues = prices.map(p => {

        if (!optimalPeriod) {
            return null;
        }

        const time = new Date(p.startTime);
        const start = new Date(optimalPeriod.startTime);
        const end = new Date(optimalPeriod.endTime);

        return time >= start && time < end
            ? p.pricePerKwh
            : null;
    });


    const ctx = document.getElementById("priceChart");

    if (!ctx) {
        console.error("priceChart canvas not found");
        return;
    }


    new Chart(ctx, {
        type: "line",

        data: {
            labels: labels,

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
                        label: function (context) {

                            return context.parsed.y
                                .toFixed(2)
                                .replace(".", ",")
                                + " kr./kWh";
                        }
                    }
                }
            },

            scales: {
                y: {
                    beginAtZero: true,

                    ticks: {
                        callback: function (value) {

                            return value
                                .toFixed(2)
                                .replace(".", ",")
                                + " kr.";
                        }
                    }
                }
            }
        }
    });
});
