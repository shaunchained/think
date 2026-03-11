/* =========================================================
   ArchSim — Scenario Player
   Handles: answer selection, AJAX submit, panel reveal,
            section rendering, animations, theme toggle
   No external dependencies — vanilla JS only
   ========================================================= */

/* -------------------------------------------------------
   Theme switcher — global, available on every page
------------------------------------------------------- */
window.setTheme = function (theme) {
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('think-theme', theme);
};

(function () {
    'use strict';

    var answered = false;

    /* -------------------------------------------------------
       selectAnswer(button)
       Called when user clicks an option card
    ------------------------------------------------------- */
    window.selectAnswer = function (btn) {
        if (answered) return;
        answered = true;

        var optionId   = btn.dataset.id;
        var scenarioId = btn.dataset.scenario;
        var stepNumber = parseInt(btn.dataset.step, 10);

        // Visual: mark selected, dim others
        var allCards = document.querySelectorAll('.option-card');
        allCards.forEach(function (card) {
            card.disabled = true;
            if (card === btn) {
                card.classList.add('selected');
            } else {
                card.classList.add('dimmed');
            }
        });

        // POST answer
        var formData = new URLSearchParams();
        formData.append('stepNumber',      stepNumber);
        formData.append('selectedOptionId', optionId);

        fetch('/scenario/' + scenarioId + '/answer', {
            method:  'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body:    formData.toString()
        })
        .then(function (res) {
            if (!res.ok) throw new Error('Network response not ok');
            return res.json();
        })
        .then(function (data) {
            renderScoreTag(btn, data.tag, data.tagColor);
            if (data.bestOptionId && data.bestOptionId !== optionId) {
                highlightBestOption(data.bestOptionId);
            }
            showReaction(data.interviewerReaction);
            revealIdealAnswer(data.idealAnswer, data.isLastStep, scenarioId, data.nextStepNumber);
        })
        .catch(function (err) {
            console.error('Answer submission failed:', err);
            answered = false; // allow retry
        });
    };

    /* -------------------------------------------------------
       renderScoreTag — shows tag badge on selected card
    ------------------------------------------------------- */
    function renderScoreTag(btn, tag, tagColor) {
        var badge = btn.querySelector('.option-badge');
        if (!badge) return;
        var span = document.createElement('span');
        span.className = 'score-tag tag-' + (tagColor || 'gray');
        span.textContent = tag;
        badge.parentNode.insertBefore(span, badge.nextSibling);
    }

    /* -------------------------------------------------------
       highlightBestOption — reveals the ideal answer card
       when the user picked something else
    ------------------------------------------------------- */
    function highlightBestOption(bestId) {
        var allCards = document.querySelectorAll('.option-card');
        allCards.forEach(function (card) {
            if (card.dataset.id === bestId) {
                card.classList.remove('dimmed');
                card.classList.add('best-answer');
                var badge = card.querySelector('.option-badge');
                var label = document.createElement('span');
                label.className = 'score-tag tag-green best-answer-label';
                label.textContent = 'Best Answer';
                if (badge) badge.parentNode.insertBefore(label, badge.nextSibling);
            }
        });
    }

    /* -------------------------------------------------------
       showReaction — fades in interviewer reaction text
    ------------------------------------------------------- */
    function showReaction(text) {
        var box  = document.getElementById('interviewerReaction');
        var span = document.getElementById('reactionText');
        if (!box || !span) return;
        span.textContent = text;
        box.style.display = 'flex';
    }

    /* -------------------------------------------------------
       revealIdealAnswer — animates right panel open
    ------------------------------------------------------- */
    function revealIdealAnswer(idealAnswer, isLastStep, scenarioId, nextStepNumber) {
        var emptyEl   = document.getElementById('panelEmpty');
        var contentEl = document.getElementById('idealContent');
        if (!emptyEl || !contentEl) return;

        emptyEl.style.display   = 'none';
        contentEl.style.display = 'block';

        // Build HTML from sections
        var html = buildIdealAnswerHTML(idealAnswer, isLastStep, scenarioId, nextStepNumber);
        contentEl.innerHTML = html;

        // Stagger section animations
        var sections = contentEl.querySelectorAll('.ia-section');
        sections.forEach(function (section, i) {
            section.style.animationDelay = (i * 120) + 'ms';
        });
    }

    /* -------------------------------------------------------
       buildIdealAnswerHTML — renders section types to HTML
    ------------------------------------------------------- */
    function buildIdealAnswerHTML(ia, isLastStep, scenarioId, nextStepNumber) {
        if (!ia || !ia.sections) return '<p>No ideal answer available.</p>';

        var parts = [];

        ia.sections.forEach(function (section) {
            var html = '<div class="ia-section">';

            switch (section.type) {

                case 'text':
                    html += '<div class="ia-section-heading">' + esc(section.heading) + '</div>';
                    html += '<div class="ia-text-content">' + esc(section.content || '') + '</div>';
                    break;

                case 'architecture-block':
                    html += '<div class="ia-section-heading">' + esc(section.heading) + '</div>';
                    html += '<div class="ia-arch-block"><pre>' + esc(section.content || '') + '</pre></div>';
                    break;

                case 'key-principles':
                    html += '<div class="ia-section-heading">' + esc(section.heading) + '</div>';
                    html += '<ol class="ia-principles-list">';
                    (section.items || []).forEach(function (item, i) {
                        html += '<li><span class="principle-num">' + (i + 1) + '</span><span>' + esc(item) + '</span></li>';
                    });
                    html += '</ol>';
                    break;

                case 'warning':
                    html += '<div class="ia-warning">';
                    html += '<div class="ia-warning-heading">⚠ ' + esc(section.heading) + '</div>';
                    html += '<div class="ia-warning-content">' + esc(section.content || '') + '</div>';
                    html += '</div>';
                    break;

                case 'tradeoffs':
                    html += '<div class="ia-section-heading">' + esc(section.heading) + '</div>';
                    html += '<div class="ia-tradeoffs">';
                    html += '<div class="ia-tradeoffs-col">';
                    html += '<div class="ia-tradeoffs-header pros">✓ Advantages</div>';
                    html += '<ul class="ia-tradeoffs-list">';
                    (section.pros || []).forEach(function (pro) {
                        html += '<li><span class="trade-icon-pro">✓</span><span>' + esc(pro) + '</span></li>';
                    });
                    html += '</ul></div>';
                    html += '<div class="ia-tradeoffs-col">';
                    html += '<div class="ia-tradeoffs-header cons">✗ Trade-offs</div>';
                    html += '<ul class="ia-tradeoffs-list">';
                    (section.cons || []).forEach(function (con) {
                        html += '<li><span class="trade-icon-con">✗</span><span>' + esc(con) + '</span></li>';
                    });
                    html += '</ul></div>';
                    html += '</div>';
                    break;

                default:
                    break;
            }

            html += '</div>';
            parts.push(html);
        });

        // One-line summary
        if (ia.oneLineSummary) {
            parts.push('<div class="ia-summary-line ia-section">"' + esc(ia.oneLineSummary) + '"</div>');
        }

        // Next / Score button
        var btnText, btnHref;
        if (isLastStep) {
            btnText = 'See Your Score →';
            btnHref = '/scenario/' + scenarioId + '/score';
        } else {
            btnText = 'Next Question →';
            btnHref = '/scenario/' + scenarioId + '/step/' + nextStepNumber;
        }
        parts.push('<a href="' + btnHref + '" class="btn-next ia-section">' + btnText + '</a>');

        return parts.join('');
    }

    /* -------------------------------------------------------
       esc — HTML escape helper (prevents XSS)
    ------------------------------------------------------- */
    function esc(str) {
        if (!str) return '';
        return String(str)
            .replace(/&/g,  '&amp;')
            .replace(/</g,  '&lt;')
            .replace(/>/g,  '&gt;')
            .replace(/"/g,  '&quot;')
            .replace(/'/g,  '&#39;');
    }

})();
