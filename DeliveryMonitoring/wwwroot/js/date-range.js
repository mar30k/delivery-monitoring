var DateRange = (function () {
    let serverNowPromise;
     function  create(selector) {

        let startDate;
        let endDate;
        let isClear = false;
        let serverNow;

        async function init() {
            serverNow = await getServerNow();

            startDate = serverNow.clone().startOf('day');
            endDate = serverNow.clone().endOf('day');
            js(selector).daterangepicker({
                startDate: startDate,
                endDate: endDate,
                maxDate: moment(),
                autoUpdateInput: true,
                locale: {
                    cancelLabel: 'Clear',
                    format: 'YYYY-MM-DD'
                }
            });

            js(selector).val(
                startDate.format('YYYY-MM-DD') + ' to ' + endDate.format('YYYY-MM-DD')
            );

            bindEvents();
        }
        function bindEvents() {
            js(selector).on('apply.daterangepicker', function (ev, picker) {
                startDate = picker.startDate.startOf('day');
                endDate = picker.endDate.endOf('day');
                isClear = false;

                js(this).val(
                    picker.startDate.format('YYYY-MM-DD') +
                    ' to ' +
                    picker.endDate.format('YYYY-MM-DD')
                );

                js(this).trigger('daterange:changed');
            });

            js(selector).on('cancel.daterangepicker', function (ev, picker) {
                picker.setStartDate(moment().startOf('day'));
                picker.setEndDate(moment().startOf('day'));

                js(this).val('');

                startDate = null;
                endDate = null;
                isClear = true;

                js(this).trigger('daterange:changed');
            });
        }

        function applyToAjax(d) {
            if (!isClear && startDate && endDate) {
                d.startDate = startDate.format("YYYY-MM-DD");
                d.endDate = endDate.format("YYYY-MM-DD");
                d.isClear = false;
            } else {
                d.startDate = null;
                d.endDate = null;
                d.isClear = true;
            }
        }

        function getRange() {
            return {
                start: startDate,
                end: endDate,
                isClear: isClear
            };
        }
        async function getServerNow() {
            if (!serverNowPromise) {
                serverNowPromise = fetch('/serverTime')
                    .then(async res => {
                        if (!res.ok) throw new Error('Failed to fetch server time');
                        const data = await res.json();
                        return moment.utc(data.serverUtcNow);
                    })
                    .catch(err => {
                        console.warn("Server time fetch failed, using client time:", err);
                        return moment.utc();
                    });
            }
            return serverNowPromise;
        }
        return {
            init,
            applyToAjax,
            getRange
        };
    }

    return {
        create
    };

})();
