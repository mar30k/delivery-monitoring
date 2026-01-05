fetch('/Map/GetMapData')
            .then(response => response.text())
            .then(script => {
                // Execute the received script
                eval(script);
            })
            .catch(error => console.error('Error loading Google Maps API:', error));

var js = jQuery.noConflict(true);
var tablelist; // Define the variable globally
var map;
const driverMarkerMap = new Map();
var headerFilterColumns = [
    { index: 1, name: 'Company' },
    { index: 4, name: 'Status' }
];
js( ()=> {
    if (!js.fn.DataTable.isDataTable('#tablelist')) {
        tablelist = js('#tablelist').DataTable({
            responsive: true,
            pageLength: 25,
            "lengthMenu": [[10, 13, 25, 50, 100], [10, 13, 25, 50, 100]],
            columnDefs: [
                {
                    orderable: false, targets: [3, 1, 7]
                },
                {
                    targets: headerFilterColumns.map(col => col.index),
                    orderable: true,
                    render: function (data, type, row) {
                        if (type === 'sort') {
                            return data;
                        }
                        return data;
                    }
                },
                {
                    orderSequence: ['asc', 'desc'],
                    targets: '_all'
                }
            ],
            order: [[4, 'desc']],
            
            initComplete: function () {
                var dt = this;
                headerFilterColumns.forEach(function (col) {
                    initHeaderFilterDropdown(dt, col.index, col.name);
                });
            }
        });
    } else {
        tablelist = js('#tablelist').DataTable();
    }
});
const statusColors = {
    offline:         { color: "#dc3545",    priority: "1" },  // Bootstrap danger - Offline
    default:         { color: "#ffc107",    priority: "2" },  // Bootstrap warning - Default
    completed:       { color: "#20c997",    priority: "3" },  // Bootstrap teal - Completed
    arrived:         { color: "#F7BEA2",    priority: "4" },  // Bootstrap info - Arrived
    arrivedatbranch: { color: "coral",      priority: "5" },  // Bootstrap info - Arrived
    accepted:        { color: "seagreen",   priority: "6" },  // Bootstrap primary - Accepted
    delivering:      { color: "darkorange", priority: "7" },  // Bootstrap orange - Delivering
    ready:           { color: "#28a745",    priority: "8" }   // Bootstrap success - Ready
};


function initMap() {
    // Default center coordinates
    const defaultLat = 9.0003776; 
    const defaultLng = 38.7828502; 

    map = new google.maps.Map(document.getElementById("map"), {
        center: { lat: defaultLat, lng: defaultLng },
        zoom: 13,
        mapTypeId: google.maps.MapTypeId.HYBRID
    });
    // map.setCenter();
    fetchDataAndUpdateMarkers();
    setInterval(fetchDataAndUpdateMarkers, 10000);
}
function fetchDataAndUpdateMarkers() {
        // index++;
    fetch(`/Driver/LiveLocation`)
        .then(response => response.json())
        .then(data => {
            // Iterate over each object in the array
            var places = [];
            data.forEach(driver => {
                // Extract the relevant information from each object
                if (driver.latLng && typeof driver.latLng === 'object' && driver.latLng.lat !== undefined && driver.latLng.lng !== undefined) {
                    var placeInfo = {
                        Name: driver.firstName,
                        lat: driver.latLng.lat,
                        lng: driver.latLng.lng,
                        Phone: driver.phoneNumber,
                        LastUpdatedAt: driver.updatedAt,
                        Status: driver.status
                    };
                }
                places.push(placeInfo);
            })
            // Push the extracted information into the places array
            // Filter out undefined values
            places = places.filter(place => place !== undefined);
            // Process the places array after fetching the data
            // ---- UPDATE DRIVER STATUS IN TABLE ----

            data.forEach(driver => {
                let rowIndex = tablelist.rows().eq(0).filter(function (index) {
                    let cellText = tablelist.cell(index, 3).node().innerText.trim();
                    return cellText === driver.phoneNumber?.trim();
                });

                if (rowIndex.length) {

                    // =====================
                    // Update Order Cell
                    // =====================
                    const orderDetailUrl = (driver.assignedOrders || [])
                        .map(code => `<a target="_blank" href="/order/${code}">${code}</a>`)
                        .join('<br>');

                    tablelist.cell(rowIndex, 7).data(orderDetailUrl);
                    // =====================
                    // Update STATUS cell
                    // =====================
                    tablelist.cell(rowIndex, 4).data(driver.status);
                    const statuscell = tablelist.cell(rowIndex, 4).node();

                    const driverStatus = (driver.status || "default").toLowerCase();
                    const statusInfo = statusColors[driverStatus] || statusColors.default;

                    statuscell.style.backgroundColor = statusInfo.color;
                    $(statuscell).attr('data-order', statusInfo.priority);

                    // ===========================
                    // Update LAST UPDATED cell
                    // ===========================
                    let lastUpdatedCell = tablelist.cell(rowIndex, 1).node();
                    $(lastUpdatedCell).attr('data-order', driver.updatedAt);
                    tablelist.cell(rowIndex, 2).data(driver.lastUpdatedAtIso);

                    // ===========================
                    // ✅ Update BATTERY cell
                    // ===========================
                    const batteryValue = driver.batteryStatus;
                    const batteryCell = tablelist.cell(rowIndex, 6).node();

                    $(batteryCell).attr("data-order", batteryValue ?? -1);

                    const batteryHtml = renderBatteryCell(batteryValue);
                    tablelist.cell(rowIndex, 6).data(batteryHtml);

                    // ===========================
                    // Refresh row
                    // ===========================
                    tablelist.row(rowIndex).invalidate();
                }
            });

            let currentPage = tablelist.page.info().page;

            // Order by the first column (1) in descending order
            tablelist.order(tablelist.order()).draw(false);

            // Go back to the original page
            tablelist.page(currentPage).draw(false);
            // ---- UPDATE MAP MARKERS ----

            places.forEach(place => {
                const existing = driverMarkerMap.get(place.Phone);

                // Check if position changed
                if (
                    !existing ||
                    existing.lat !== place.lat ||
                    existing.lng !== place.lng
                ) {
                    // If marker exists, remove old one
                    if (existing?.marker) {
                        existing.marker.setMap(null);
                    }

                    // Create new marker
                    const newMarker = new google.maps.Marker({
                        position: { lat: place.lat, lng: place.lng },
                        title: place.Name,
                        map: map,
                        icon: {
                            url: place.Status !== "offline" ? '/images/motor.png' : '/images/offline_motors.png',
                            scaledSize: new google.maps.Size(51, 61)
                        }
                    });

                    // Update InfoWindow events
                    const content = `<b>Name:</b> ${place.Name}<br/><b>Phone:</b> ${place.Phone}<br/><b>Status:</b> ${place.Status}`;
                    const infowindow = new google.maps.InfoWindow();

                    google.maps.event.addListener(newMarker, 'mouseover', function () {
                        infowindow.setContent(content);
                        infowindow.open(map, newMarker);
                    });
                    google.maps.event.addListener(newMarker, 'mouseout', function () {
                        infowindow.close();
                    });

                    // Store in the map
                    driverMarkerMap.set(place.Phone, {
                        marker: newMarker,
                        lat: place.lat,
                        lng: place.lng
                    });
                }
            });


        })
        .catch(error => console.error('Error fetching live location:', error, error.responseText));

}
function batteryColor(battery) {
    if (battery == null) return "#777777"; // gray

    if (battery < 20) return "#ff0000";      // red
    if (battery < 40) return "#ff6600";      // orange
    if (battery < 60) return "#ffcc00";      // yellow
    if (battery < 80) return "#99cc00";      // yellow-green
    return "#00cc00";                        // green
}

function renderBatteryCell(battery) {
    if (battery == null) {
        return `<span style="color:#777;">N/A</span>`;
    }

    const color = batteryColor(battery);

    return `
        <span style="font-weight:600; color:${color};">
            ${battery}%
        </span>
    `;
}