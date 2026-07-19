let priceChart = null;

function getPrices() {
    const element = document.querySelector(".price-data");

    if (!element) {
        return [];
    }

    const data = JSON.parse(element.textContent);

    return data;
}

function findOptimalPeriods(prices, minutes) {
    const periods = [];
    const slots = minutes / 15;
    const now = new Date();

    for (let i = 0; i <= prices.length - slots; i++) {
        const start = new Date(prices[i].startTime);
        const end = new Date(prices[i + slots - 1].endTime);

        // Spring perioder over, der allerede er startet
        if (start < now) {
            continue;
        }

        const window = prices.slice(i, i + slots);

        const average = window.reduce((sum, price) => sum + price.pricePerKwh, 0) / slots;

        periods.push({
            startTime: prices[i].startTime,
            endTime: prices[i + slots - 1].endTime,
            averagePricePerKwh: average
        });
    }

    return periods.sort(
        (a, b) => a.averagePricePerKwh - b.averagePricePerKwh
    );
}

function formatDateTime(value) {
    const date = new Date(value);

    return `${date.toLocaleDateString("da-DK", {
        weekday: "long",
        day: "numeric",
        month: "long"
    })} kl. ${date.toLocaleTimeString("da-DK", {
        hour: "2-digit",
        minute: "2-digit"
    })}`;
}

function renderBestPeriod(period) {
    const container = document.getElementById("best-period");

    if (!period) {
        container.innerHTML = "";
        return;
    }

    const start = new Date(period.startTime);
    const end = new Date(period.endTime);

    const sameDay = start.toDateString() === end.toDateString();

    container.innerHTML = `
        <div class="alert alert-success">
            <strong>Bedste periode:</strong>
            ${formatDateTime(period.startTime)}
            –
            ${sameDay
            ? end.toLocaleTimeString("da-DK", {
                hour: "2-digit",
                minute: "2-digit"
            })
            : formatDateTime(period.endTime)}
            (${period.averagePricePerKwh.toFixed(2).replace(".", ",")} kr./kWh)
        </div>`;
}

function renderOptimalPeriods(periods) {
    const container = document.getElementById("optimal-periods");

    container.innerHTML = `
        <table class="table table-striped">
            <thead>
                <tr>
                    <th>Periode</th>
                    <th>Pris</th>
                </tr>
            </thead>
            <tbody>
                ${periods.slice(0, 10).map(period => `
                    <tr>
                        <td>
                            ${formatDateTime(period.startTime)}
                            -
                            ${new Date(period.endTime)
            .toLocaleTimeString("da-DK", {
                hour: "2-digit",
                minute: "2-digit"
            })}
                        </td>
                        <td>
                            ${period.averagePricePerKwh
            .toFixed(2)
            .replace(".", ",")} kr./kWh
                        </td>
                    </tr>
                `).join("")}
            </tbody>
        </table>`;
}

function renderChart(prices, optimalPeriod) {
    const canvas = document.querySelector(".price-chart");

    if (!canvas) {
        return;
    }

    const labels = prices.map(price =>
        new Date(price.startTime)
            .toLocaleTimeString("da-DK", {
                hour: "2-digit",
                minute: "2-digit"
            }));

    const values = prices.map(
        price => price.pricePerKwh
    );

    const currentIndex = findCurrentIndex(prices);

    const optimalValues = prices.map(price => {

        if (!optimalPeriod) {
            return null;
        }

        const time = new Date(price.startTime);
        const start = new Date(optimalPeriod.startTime);
        const end = new Date(optimalPeriod.endTime);

        return time >= start && time <= end
            ? price.pricePerKwh
            : null;
    });


    if (priceChart) {
        priceChart.data.datasets[1].data = optimalValues;
        priceChart.update();
        return;
    }

    const darkGreen = "#198754";
    const lightBlue = "#64B5F6";

    priceChart = new Chart(canvas, {
        type: "line",

        data: {
            labels,

            datasets: [
                {
                    label: "Pris kr./kWh",
                    data: values,
                    tension: 0.2,
                    borderWidth: 2,
                    pointRadius: ctx =>
                        ctx.dataIndex < currentIndex ? 0 : 2,
                    borderColor: lightBlue,
                    backgroundColor: lightBlue,

                    segment: {
                        borderColor: ctx =>
                            ctx.p0DataIndex < currentIndex
                                ? "rgba(100, 181, 246, 0.25)"
                                : lightBlue
                    },

                    pointBackgroundColor: ctx =>
                        ctx.dataIndex < currentIndex
                            ? "rgba(100, 181, 246, 0.25)"
                            : lightBlue
                },
                {
                    label: "Billigste periode",
                    data: optimalValues,
                    tension: 0.2,
                    borderWidth: 5,
                    pointRadius: 5,
                    spanGaps: false,
                    borderColor: darkGreen,
                    backgroundColor: darkGreen,
                    pointBackgroundColor: darkGreen,
                    pointBorderColor: darkGreen
                }]
        },

        options: {
            responsive: true,

            plugins: {
                annotation: {
                    annotations: {
                        currentTime: {
                            type: "line",
                            xMin: currentIndex,
                            xMax: currentIndex,
                            borderColor: "#757575",
                            borderWidth: 2,
                            borderDash: [5, 5],
                            label: {
                                display: true,
                                content: "Nu",
                                position: "start"
                            }
                        }
                    }
                },
                tooltip: {
                    callbacks: {
                        label(context) {
                            return `${context.parsed.y
                                .toFixed(2)
                                .replace(".", ",")} kr./kWh`;
                        }
                    }
                }
            }
        }
    });
}


function updateOptimalPeriod() {
    const hours = Number(document.getElementById("hours").value);
    const minutes = Number(document.getElementById("minutes").value);
    const totalMinutes = hours * 60 + minutes;

    if (totalMinutes < 5 || totalMinutes > 1440) {
        return;
    }

    const prices = getPrices();
    const periods = findOptimalPeriods(prices, totalMinutes);
    const cheapest = periods[0];

    renderBestPeriod(cheapest);
    renderOptimalPeriods(periods);
    renderChart(prices, cheapest);
}

function findCurrentIndex(prices) {
    const now = new Date();

    return prices.findIndex(price =>
        new Date(price.endTime) >= now
    );
}

function isPastSegment(context) {
    return context.p0DataIndex < findCurrentIndex(getPrices());
}

document.addEventListener("DOMContentLoaded", () => {
    const hoursInput = document.getElementById("hours");
    const minutesInput = document.getElementById("minutes");

    console.log("price-chart.js initialized");
    console.log({ hoursInput, minutesInput });

    hoursInput?.addEventListener("change", () => {
        console.log("hours changed");
        updateOptimalPeriod();
    });

    minutesInput?.addEventListener("change", () => {
        console.log("minutes changed");
        updateOptimalPeriod();
    });

    updateOptimalPeriod();
});
