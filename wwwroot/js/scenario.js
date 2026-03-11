/* ============================================================
   THINK — Engineer Scenario Simulator
   scenario.js
   ============================================================ */

/* ── PROGRESS BAR ANIMATION ── */
(function animateProgressBar() {
    var fill = document.getElementById('progressFill');
    if (!fill) return;
    var target = fill.getAttribute('data-target');
    if (!target) return;
    // Small delay so the CSS transition is visible
    requestAnimationFrame(function () {
        setTimeout(function () {
            fill.style.width = target;
        }, 80);
    });
})();

/* ── SMOOTH SCROLL FOR HERO CTA ── */
(function bindHeroScroll() {
    var btn = document.querySelector('a[href="#scenarios"]');
    if (!btn) return;
    btn.addEventListener('click', function (e) {
        e.preventDefault();
        var target = document.getElementById('scenarios');
        if (target) target.scrollIntoView({ behavior: 'smooth', block: 'start' });
    });
})();

/* ── ANSWER SUBMISSION ── */
var answerSubmitted = false;

function submitAnswer(button) {
    if (answerSubmitted) return;
    answerSubmitted = true;

    var selectedIndex = parseInt(button.getAttribute('data-index'), 10);
    var scenarioId    = button.getAttribute('data-scenario-id');
    var stepNumber    = parseInt(button.getAttribute('data-step-number'), 10);

    // Disable all option buttons immediately — prevent double-submit
    var allButtons = document.querySelectorAll('.option-btn');
    allButtons.forEach(function (btn) { btn.disabled = true; });

    fetch('/Scenario/Submit', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: new URLSearchParams({
            id: scenarioId,
            stepNumber: stepNumber,
            selectedIndex: selectedIndex
        })
    })
    .then(function (response) {
        if (!response.ok) throw new Error('Server returned ' + response.status);
        return response.json();
    })
    .then(function (data) {
        if (data.error) {
            showError(data.error);
            return;
        }
        renderFeedback(data, selectedIndex);
    })
    .catch(function (err) {
        console.error('Submit failed:', err);
        showError('Something went wrong. Please refresh and try again.');
        // Re-enable buttons so user is not stuck
        allButtons.forEach(function (btn) { btn.disabled = false; });
        answerSubmitted = false;
    });
}

function renderFeedback(data, selectedIndex) {
    var allButtons = document.querySelectorAll('.option-btn');

    // Apply correct / wrong visual states
    allButtons.forEach(function (btn) {
        var idx = parseInt(btn.getAttribute('data-index'), 10);
        if (idx === data.correctIndex) {
            btn.classList.add('option-correct');
        } else if (idx === selectedIndex && !data.isCorrect) {
            btn.classList.add('option-wrong');
        }
    });

    // Populate explanation
    var explanationBlock = document.getElementById('explanationBlock');
    if (explanationBlock) {
        explanationBlock.textContent = data.explanation;
    }

    // Populate senior quote
    var seniorQuote = document.getElementById('seniorQuote');
    if (seniorQuote) {
        seniorQuote.textContent = '\u201C' + data.seniorSays + '\u201D';
    }

    // Set the Next button href from the server response — this is the authoritative URL
    var nextBtn = document.getElementById('nextBtn');
    if (nextBtn && data.nextUrl) {
        nextBtn.href = data.nextUrl;
    }

    // Reveal the feedback section
    var feedbackBlock = document.getElementById('feedbackBlock');
    if (feedbackBlock) {
        feedbackBlock.style.display = 'block';
        // Scroll feedback into view smoothly
        setTimeout(function () {
            feedbackBlock.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        }, 100);
    }
}

function showError(message) {
    var feedbackBlock = document.getElementById('feedbackBlock');
    var explanationBlock = document.getElementById('explanationBlock');
    if (explanationBlock) {
        explanationBlock.textContent = message;
        explanationBlock.style.borderLeftColor = '#ef4444';
    }
    if (feedbackBlock) {
        feedbackBlock.style.display = 'block';
    }
}
