(function () {
    'use strict';

    var _raf = null;
    var _ro  = null;
    var _mo  = null;

    function getColor() {
        try {
            var bg = getComputedStyle(document.documentElement).getPropertyValue('--bg').trim();
            return /^#[dDeEfF]/i.test(bg)
                ? 'rgba(22,199,132,0.6)'   // light theme
                : 'rgba(22,199,132,0.48)'; // dark theme
        } catch(e) {
            return 'rgba(22,199,132,0.48)';
        }
    }

    function startGrid(canvas) {
        if (_raf) { cancelAnimationFrame(_raf); _raf = null; }
        if (_ro)  { _ro.disconnect(); _ro = null; }

        var ctx    = canvas.getContext('2d');
        var SIZE   = 44;
        var SPEED  = 0.5;
        var offset = 0;

        function resize() {
            var parent = canvas.parentElement;
            if (!parent) return;
            var rect = parent.getBoundingClientRect();
            if (rect.width === 0 || rect.height === 0) return;
            var dpr = window.devicePixelRatio || 1;
            canvas.width        = Math.round(rect.width  * dpr);
            canvas.height       = Math.round(rect.height * dpr);
            canvas.style.width  = rect.width  + 'px';
            canvas.style.height = rect.height + 'px';
            ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
        }

        function draw() {
            var dpr = window.devicePixelRatio || 1;
            var w   = canvas.width  / dpr;
            var h   = canvas.height / dpr;
            if (!w || !h) { _raf = requestAnimationFrame(draw); return; }

            ctx.clearRect(0, 0, w, h);
            ctx.beginPath();
            ctx.strokeStyle = getColor();
            ctx.lineWidth   = 1;

            var o = offset % SIZE;
            for (var x = o - SIZE; x < w + SIZE; x += SIZE) {
                ctx.moveTo(Math.floor(x) + 0.5, 0);
                ctx.lineTo(Math.floor(x) + 0.5, h);
            }
            for (var y = o - SIZE; y < h + SIZE; y += SIZE) {
                ctx.moveTo(0, Math.floor(y) + 0.5);
                ctx.lineTo(w, Math.floor(y) + 0.5);
            }
            ctx.stroke();
            offset += SPEED;
            _raf = requestAnimationFrame(draw);
        }

        resize();
        draw();

        _ro = new ResizeObserver(function() {
            cancelAnimationFrame(_raf);
            resize();
            draw();
        });
        if (canvas.parentElement) _ro.observe(canvas.parentElement);
    }

    function findAndStart() {
        var canvas = document.getElementById('hdaGrid');
        if (canvas) {
            if (_mo) { _mo.disconnect(); _mo = null; }
            startGrid(canvas);
        }
    }

    function watchForCanvas() {
        findAndStart();
        if (document.getElementById('hdaGrid')) return;
        // Not found — watch DOM until it appears
        _mo = new MutationObserver(function() { findAndStart(); });
        _mo.observe(document.body || document.documentElement, { childList: true, subtree: true });
    }

    // Start immediately
    if (document.readyState !== 'loading') {
        watchForCanvas();
    } else {
        document.addEventListener('DOMContentLoaded', watchForCanvas);
    }

    // Re-run after Blazor navigation
    document.addEventListener('enhancedload', watchForCanvas);
})();
