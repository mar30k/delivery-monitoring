export const DashboardMap = (() => {

    let map;
    const markerMap = new Map();

    function initMap() {
        map = new google.maps.Map(document.getElementById("map"), {
            center: { lat: 9.0003776, lng: 38.7828502 },
            zoom: 13,
            mapTypeId: google.maps.MapTypeId.HYBRID
        });

        refresh();
        setInterval(refresh, 20000);
    }

    async function refresh(drivers) {
        if (!map || !Array.isArray(drivers)) return;

        drivers.forEach(d => {
            if (!d.latLng) return;

            const key = d.phoneNumber;
            const old = markerMap.get(key);

            if (old) old.marker.setMap(null);

            const marker = new google.maps.Marker({
                position: d.latLng,
                map,
                icon: {
                    url: d.status !== "offline"
                        ? '/images/motor.png'
                        : '/images/offline_motors.png',
                    scaledSize: new google.maps.Size(51, 61)
                }
            });

            markerMap.set(key, { marker });
        });
    }

    return { initMap, refresh };
})();
