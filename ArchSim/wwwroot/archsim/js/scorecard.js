/* =========================================================
   ArchSim — Scorecard Animations
   SVG circle draw + score count-up
   ========================================================= */

(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        var data = window.SCORE_DATA;
        if (!data) return;

        animateCircle(data.dashOffset);
        animateCount(data.finalScore);
    });

    function animateCircle(targetOffset) {
        var el = document.getElementById('scoreFill');
        if (!el) return;
        // Start at full offset (empty circle), animate to target
        requestAnimationFrame(function () {
            el.style.transition = 'stroke-dashoffset 0.8s ease';
            el.style.strokeDashoffset = targetOffset;
        });
    }

    function animateCount(target) {
        var el = document.getElementById('scoreNumber');
        if (!el) return;

        var start     = 0;
        var duration  = 600;
        var startTime = null;

        function step(timestamp) {
            if (!startTime) startTime = timestamp;
            var progress = Math.min((timestamp - startTime) / duration, 1);
            var eased    = 1 - Math.pow(1 - progress, 3); // ease-out cubic
            el.textContent = Math.round(eased * target);
            if (progress < 1) requestAnimationFrame(step);
        }

        requestAnimationFrame(step);
    }

})();
