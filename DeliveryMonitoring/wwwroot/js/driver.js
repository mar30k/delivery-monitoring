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
        var markers = [];
        js(document).ready(function () {
            if (!js.fn.DataTable.isDataTable('#tablelist')) {
                tablelist = js('#tablelist').DataTable({
                    responsive: true,
                    columnDefs: [
                        { orderable: false, targets: [2, 4] } // Disable sorting on specified columns
                    ],
                    order: [[3, 'desc']]
                });
            } else {
                tablelist = js('#tablelist').DataTable();
            }
        });
        const statusColors = {
            offline: { color: "#dc3545",     priority: "2" },
            ready:     { color: "#43A047",   priority: "5" },
            accepted: { color: "#0d6efd",    priority: "3" },
            delivering: { color: "#fd7e14",      priority: "2" },
            default: { color: "#ffc107",   priority: "1" }
        };


        function initMap() {
            // Default center coordinates
            const defaultLat = 8.9660573;; // Replace with your default latitude
            const defaultLng = 38.8404793; // Replace with your default longitude

            map = new google.maps.Map(document.getElementById("map"), {
                center: { lat: defaultLat, lng: defaultLng },
                zoom: 13,
                mapTypeId: google.maps.MapTypeId.ROADMAP
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
                                    LastUpdatedAt: driver.lastUpdatedAt,
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
                                let cellText = tablelist.cell(index, 2).node().innerText.trim();
                                return cellText === driver.phoneNumber?.trim();
                            });

                            if (rowIndex.length) {
                                // Update the data
                                tablelist.cell(rowIndex, 3).data(driver.status);
                                        // Set the text
                                tablelist.cell(rowIndex, 3).data(driver.status);

                                // Get the actual cell element
                                const statuscell = tablelist.cell(rowIndex, 3).node();

                                const driverStatus = driver.status || "default";
                                const statusInfo = statusColors[driverStatus] || statusColors.default;

                                const bgColor = statusInfo.color;
                                const priority = statusInfo.priority;

                                // Apply background color
                                statuscell.style.backgroundColor = bgColor;
                                $(statuscell).attr('data-order', priority);
                                // Get the cell for last updated
                                let lastUpdatedCell = tablelist.cell(rowIndex, 1).node();

                                // Update the data-order attribute and cell content
                                $(lastUpdatedCell).attr('data-order', driver.lastUpdatedAt);
                                tablelist.cell(rowIndex, 1).data(driver.lastUpdatedAtIso);

                                // Invalidate row data for DataTables
                                tablelist.row(rowIndex).invalidate();
                            }
                        });

                        let currentPage = tablelist.page.info().page;

                        // Order by the first column (1) in descending order
                        tablelist.order([[3, 'desc']]).draw(false);

                        // Go back to the original page
                        tablelist.page(currentPage).draw(false);
                        // ---- UPDATE MAP MARKERS ----

                        markers.forEach(marker => marker.setMap(null));
                        markers = []; // Reset the array

                        places.forEach(place => {
                            const marker = new google.maps.Marker({
                                position: { lat: place.lat, lng: place.lng },
                                title: place.Name,
                                map: map,
                                icon: {
                                    url: place.Status!== "offline"? '/images/motor.png' : '/images/offline_motors.png',
                                    scaledSize: new google.maps.Size(51, 61) // Adjust the size as needed
                                }
                            });

                            markers.push(marker);

                            // Add a listener for the marker
                            var content = "<b>Name:</b> " + place.Name + "<br/><b>Phone:</b> " + place.Phone + "<br/><b>Status:</b> " + place.Status;

                            var infowindow = new google.maps.InfoWindow();

                            google.maps.event.addListener(marker, 'mouseover', (function (marker, content, infowindow) {
                                return function () {
                                    infowindow.setContent(content);
                                    infowindow.open(map, marker);
                                };
                            })(marker, content, infowindow));

                            google.maps.event.addListener(marker, 'mouseout', (function (infowindow) {
                                return function () {
                                    infowindow.close();
                                };
                            })(infowindow));
                        });
                        // console.log(places);
                        // console.log(index);

                    })
                    .catch(error => console.error('Error fetching live location:', error, error.responseText));

                    tablelist.order([3, 'desc']).draw(false);
}
