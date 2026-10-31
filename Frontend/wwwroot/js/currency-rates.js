(() => {
    "use strict";

    const body = document.getElementById("ratesBody");
    const status = document.getElementById("ratesStatus");
    const refresh = document.getElementById("refreshRates");
    const amount = document.getElementById("uahAmount");
    const select = document.getElementById("currencySelect");
    const output = document.getElementById("convertedAmount");
    let rates = [];

    function renderConversion() {
        const rate = rates.find(x => x.currency === select.value);
        const sum = Number(amount.value);
        if (!rate || !Number.isFinite(sum) || sum < 0 || Number(rate.sale) <= 0) {
            output.textContent = "—";
            return;
        }
        const value = sum / Number(rate.sale);
        output.textContent = new Intl.NumberFormat("en-US", {
            minimumFractionDigits: 2, maximumFractionDigits: 2
        }).format(value) + " " + rate.currency;
    }

    async function loadRates() {
        refresh.disabled = true;
        status.textContent = "Loading...";
        try {
            const response = await fetch("/CurrencyRates/Data", { cache: "no-store" });
            if (!response.ok) throw new Error("HTTP " + response.status);
            const data = await response.json();
            if (!Array.isArray(data.rates) || data.rates.length === 0)
                throw new Error("No currency rates received");
            rates = data.rates.filter(x => x.baseCurrency === "UAH" && Number(x.sale) > 0);
            if (rates.length === 0) throw new Error("No UAH exchange rates received");

            body.replaceChildren();
            select.replaceChildren();
            for (const rate of rates) {
                const tr = document.createElement("tr");
                for (const value of [rate.currency, rate.baseCurrency, rate.buy, rate.sale]) {
                    const td = document.createElement("td");
                    td.textContent = String(value);
                    tr.appendChild(td);
                }
                body.appendChild(tr);
                const option = document.createElement("option");
                option.value = rate.currency;
                option.textContent = rate.currency;
                select.appendChild(option);
            }
            select.disabled = false;
            status.textContent = "Exchange rates loaded from PrivatBank.";
            renderConversion();
        } catch (error) {
            console.error("Currency API failure", error);
            status.textContent = "Unable to load exchange rates. Try again later.";
            body.replaceChildren();
            const tr = document.createElement("tr");
            const td = document.createElement("td");
            td.colSpan = 4;
            td.textContent = "Currency rates are temporarily unavailable.";
            tr.appendChild(td);
            body.appendChild(tr);
            select.disabled = true;
            output.textContent = "—";
            rates = [];
        } finally {
            refresh.disabled = false;
        }
    }

    refresh.addEventListener("click", loadRates);
    amount.addEventListener("input", renderConversion);
    select.addEventListener("change", renderConversion);
    loadRates();
})();