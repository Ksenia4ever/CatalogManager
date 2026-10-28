"use strict";

(() => {
    const mapElement = document.getElementById("storesMap");
    const searchInput = document.getElementById("storeSearch");
    const refreshButton = document.getElementById("refreshStores");
    const statusElement = document.getElementById("mapStatus");

    if (!mapElement || !searchInput || !refreshButton || !statusElement) return;
    if (typeof L === "undefined") {
        statusElement.textContent = "Map library could not be loaded. Check your internet connection.";
        return;
    }

    const map = L.map(mapElement).setView([50.4501, 30.5234], 11);
    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        maxZoom: 19,
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    const markersLayer = L.featureGroup().addTo(map);
    let stores = [];

    const validCoordinate = store =>
        Number.isFinite(store.latitude) && Number.isFinite(store.longitude) &&
        store.latitude >= -90 && store.latitude <= 90 &&
        store.longitude >= -180 && store.longitude <= 180 &&
        !(store.latitude === 0 && store.longitude === 0);

    function renderStores() {
        markersLayer.clearLayers();
        const query = searchInput.value.trim().toLocaleLowerCase();
        const filtered = stores.filter(store =>
            String(store.name ?? "").toLocaleLowerCase().includes(query));
        const visible = filtered.filter(validCoordinate);

        for (const store of visible) {
            // DOM nodes (textContent) prevent HTML injection through API data.
            const popup = document.createElement("div");
            const name = document.createElement("strong");
            name.textContent = store.name || "Unnamed store";
            const address = document.createElement("div");
            address.textContent = store.address || "No address provided";
            const details = document.createElement("a");
            details.href = `/Stores/Details/${encodeURIComponent(store.id)}`;
            details.textContent = "Store details";
            popup.append(name, address, details);

            L.marker([store.latitude, store.longitude])
                .bindPopup(popup)
                .addTo(markersLayer);
        }

        if (visible.length > 0) {
            map.fitBounds(markersLayer.getBounds().pad(0.15), { maxZoom: 14 });
        }

        const missingCoordinates = filtered.length - visible.length;
        statusElement.textContent =
            `Showing ${visible.length} of ${stores.length} stores.` +
            (missingCoordinates > 0
                ? ` ${missingCoordinates} filtered store(s) have invalid or missing coordinates.`
                : "");
    }

    async function loadStores() {
        refreshButton.disabled = true;
        statusElement.textContent = "Loading stores...";
        try {
            // B1 preserves the original JSON array response when no skip/limit is passed.
            const response = await fetch("/api/stores", {
                headers: { "Accept": "application/json" },
                cache: "no-store"
            });
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            const data = await response.json();
            if (!Array.isArray(data)) throw new Error("Unexpected API response format");
            stores = data;
            renderStores();
        } catch (error) {
            markersLayer.clearLayers();
            stores = [];
            statusElement.textContent = `Failed to load stores: ${error.message}`;
        } finally {
            refreshButton.disabled = false;
        }
    }

    searchInput.addEventListener("input", renderStores);
    refreshButton.addEventListener("click", loadStores);
    loadStores();
})();