export const DashboardScroll = (function () {

    let scrollingDown = true;
    let autoScrollActive = true;
    let isPaused = false;
    let resumeTimer;

    const speed = 3;
    const pauseTime = 3000;

    let running = false;

    function step() {
        if (running || !autoScrollActive || isPaused) return;
        running = true;

        const el = document.scrollingElement || document.documentElement;
        const bottom = el.scrollHeight - el.clientHeight;

        if (scrollingDown) {
            el.scrollTop < bottom ? el.scrollTop += speed : pause();
        } else {
            el.scrollTop > 0 ? el.scrollTop -= speed : pause();
        }

        running = false;
        requestAnimationFrame(step);
    }


    function pause() {
        isPaused = true;
        setTimeout(() => {
            scrollingDown = !scrollingDown; // flip direction
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
