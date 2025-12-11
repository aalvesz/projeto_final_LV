document.addEventListener("DOMContentLoaded", () => {
    const tempEl = document.getElementById("temperature");
    const condEl = document.getElementById("condition");
    const select = document.getElementById("cidade");

    if (!tempEl || !condEl || !select) {
        return;
    }

    async function atualizarClimaHeader(cidade) {
        if (!cidade) {
            tempEl.textContent = "--°C";
            condEl.textContent = "--";
            return;
        }

        try {
            const res = await fetch(`/Weather/ByCity?city=${encodeURIComponent(cidade)}`);
            if (!res.ok) {
                tempEl.textContent = "--°C";
                condEl.textContent = "--";
                return;
            }

            const data = await res.json();
            if (!data.ok) {
                tempEl.textContent = "--°C";
                condEl.textContent = "--";
                return;
            }

            const min = data.min != null ? Number(data.min).toFixed(1) : "--";
            const max = data.max != null ? Number(data.max).toFixed(1) : "--";

            tempEl.textContent = `${min}°C / ${max}°C`;
            condEl.textContent = data.date
                ? `Previsão ${data.date}`
                : "Previsão diária";
        } catch (e) {
            tempEl.textContent = "--°C";
            condEl.textContent = "--";
        }
    }

    // Ao carregar a página, usa o valor atual do select (que veio do cookie)
    atualizarClimaHeader(select.value);
});
