window.DashboardScroll = (function () {

    let scrollingDown = true;
    let autoScrollActive = true;
    let isPaused = false;
    let resumeTimer;

    const speed = 1;
    const pauseTime = 3000;

    function step() {
        if (!autoScrollActive || isPaused) return;

        const el = document.scrollingElement;
        const bottom = el.scrollHeight - el.clientHeight;

        if (scrollingDown) {
            if (el.scrollTop < bottom) {
                el.scrollTop += speed;
            } else pause(false);
        } else {
            if (el.scrollTop > 0) {
                el.scrollTop -= speed;
            } else pause(true);
        }

        requestAnimationFrame(step);
    }

    function pause(nextDirection) {
        isPaused = true;
        setTimeout(() => {
            scrollingDown = nextDirection;
            isPaused = false;
            step();
        }, pauseTime);
    }

    function init() {
        step();

        ["wheel", "touchstart", "mousedown", "keydown"].forEach(e => {
            window.addEventListener(e, () => {
                autoScrollActive = false;
                clearTimeout(resumeTimer);
                resumeTimer = setTimeout(() => {
                    autoScrollActive = true;
                    step();
                }, 10000);
            });
        });
    }

    return { init };
})();
