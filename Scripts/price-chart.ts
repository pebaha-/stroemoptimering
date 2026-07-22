declare const Chart: any;
let priceChart: any = null;

function getPrices(): ElectricityPrice[] {
    const element = document.getElementById("price-data");

    if (!element?.textContent) {
        return [];
    }

    return JSON.parse(element.textContent) as ElectricityPrice[];
}

function findOptimalPeriods(prices: ElectricityPrice[], minutes: number) {
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

function formatDateTime(value: string): string {
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

function formatTime(value: string): string {
    return new Date(value).toLocaleTimeString("da-DK", {
        hour: "2-digit",
        minute: "2-digit"
    });
}

function formatPeriod(startTime: string, endTime: string): string {
    const start = new Date(startTime);
    const end = new Date(endTime);
    const formattedEnd = start.toDateString() === end.toDateString()
        ? formatTime(endTime)
        : formatDateTime(endTime);

    return `${formatDateTime(startTime)} – ${formattedEnd}`;
}

function formatPrice(pricePerKwh: number): string {
    return `${pricePerKwh.toLocaleString("da-DK", {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    })} kr./kWh`;
}

function renderBestPeriod(period: OptimalPeriod | undefined): void {
    const container = document.getElementById("best-period") as HTMLElement | null;

    if (!container) {
        return;
    }

    if (!period) {
        container.innerHTML = "";
        return;
    }

    container.innerHTML = `
        <div class="alert alert-success">
            <strong>Bedste periode:</strong>
            ${formatPeriod(period.startTime, period.endTime)}
            (${formatPrice(period.averagePricePerKwh)})
        </div>`;
}

function renderOptimalPeriods(periods: OptimalPeriod[]): void {
    const container = document.getElementById("optimal-periods");

    if (!container) {
        return;
    }

    const tableTemplate = document.getElementById(
        "optimal-period-table-template"
    ) as HTMLTemplateElement | null;

    if (!tableTemplate) {
        return;
    }

    const table = tableTemplate.content
        .firstElementChild!
        .cloneNode(true) as HTMLTableElement;

    const tbody = table.querySelector("tbody");

    if (!tbody) {
        return;
    }

    const rowTemplate = document.getElementById(
        "optimal-period-row-template"
    ) as HTMLTemplateElement | null;

    if (!rowTemplate) {
        return;
    }

    for (const [index, period] of periods.slice(0, 10).entries()) {
        const row = rowTemplate.content
            .firstElementChild!
            .cloneNode(true) as HTMLTableRowElement;

        row.querySelector(".rank")!.textContent = String(index + 1);

        row.querySelector(".period")!.textContent =
            formatPeriod(period.startTime, period.endTime);

        row.querySelector(".price")!.textContent =
            formatPrice(period.averagePricePerKwh);

        tbody.appendChild(row);
    }

    container.replaceChildren(table);
}

function renderChart(
    prices: ElectricityPrice[],
    optimalPeriod: OptimalPeriod | undefined
): void {
    const canvas = document.querySelector<HTMLCanvasElement>(".price-chart");

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

        return time >= start && time < end
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
                    pointRadius: (ctx: any) =>
                        ctx.dataIndex < currentIndex ? 0 : 2,
                    borderColor: lightBlue,
                    backgroundColor: lightBlue,

                    segment: {
                        borderColor: (ctx: any) =>
                            ctx.p0DataIndex < currentIndex
                                ? "rgba(100, 181, 246, 0.25)"
                                : lightBlue
                    },

                    pointBackgroundColor: (ctx: any) =>
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
                }
            ]
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
                        label(context: any): string {
                            return formatPrice(context.parsed.y);
                        }
                    }
                }
            }
        }
    });
}

function updateOptimalPeriod(): void {
    const hoursInput = document.getElementById("hours") as HTMLInputElement | null;
    const minutesInput = document.getElementById("minutes") as HTMLSelectElement | null;

    if (!hoursInput || !minutesInput) {
        return;
    }

    const hours = Number(hoursInput.value);
    const minutes = Number(minutesInput.value);
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

function findCurrentIndex(prices: ElectricityPrice[]): number {
    const now = new Date();

    return prices.findIndex(price =>
        new Date(price.endTime) >= now
    );
}

document.addEventListener("DOMContentLoaded", () => {
    const hoursInput = document.getElementById("hours");
    const minutesInput = document.getElementById("minutes");

    hoursInput?.addEventListener("change", () => {
        updateOptimalPeriod();
    });

    minutesInput?.addEventListener("change", () => {
        updateOptimalPeriod();
    });

    updateOptimalPeriod();
});
