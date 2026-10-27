// «Слоты удачи»: анимация и звук. Результат определяет сервер (/api/spins/spin),
// клиент только проигрывает выпавшие символы.
(function () {
    'use strict';

    var config = window.spinConfig || {};
    if (!config.canSpin) {
        return;
    }

    var FUN_MODE = config.funMode === true;
    var SPIN_LABEL = FUN_MODE ? 'Крутить (фан-режим)' : 'Крутить барабаны';

    var SYMBOLS = config.symbols && config.symbols.length ? config.symbols
        : ['💎', '🍒', '🔔', '⭐', '🍀', '🍋'];
    var SPIN_URL = '/api/spins/spin';
    var REEL_SPIN_MS = 2200;   // сколько всего крутится первый барабан
    var REEL_STAGGER_MS = 700; // задержка остановки каждого следующего
    var TICK_MS = 90;          // смена символа во время вращения

    var button = document.getElementById('spinButton');
    var reels = [0, 1, 2].map(function (i) { return document.getElementById('reel' + i); });
    var resultPanel = document.getElementById('spinResult');
    var soundToggle = document.getElementById('soundToggle');

    var reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var soundEnabled = true;
    try {
        soundEnabled = localStorage.getItem('slots-sound') !== 'off';
    } catch (e) { /* приватный режим — просто оставляем вкл. */ }

    var audioCtx = null;
    var spinOscillators = [];

    // ---------- Звук через Web Audio API (без аудиофайлов и библиотек) ----------

    function ensureAudio() {
        if (!audioCtx) {
            var Ctx = window.AudioContext || window.webkitAudioContext;
            if (!Ctx) { return null; }
            audioCtx = new Ctx();
        }
        if (audioCtx.state === 'suspended') {
            audioCtx.resume();
        }
        return audioCtx;
    }

    function beep(freq, duration, type, volume, when) {
        var ctx = ensureAudio();
        if (!ctx || !soundEnabled) { return; }
        var t = ctx.currentTime + (when || 0);
        var osc = ctx.createOscillator();
        var gain = ctx.createGain();
        osc.type = type || 'square';
        osc.frequency.value = freq;
        gain.gain.setValueAtTime(volume || 0.08, t);
        gain.gain.exponentialRampToValueAtTime(0.0001, t + duration);
        osc.connect(gain).connect(ctx.destination);
        osc.start(t);
        osc.stop(t + duration + 0.02);
    }

    // рокот вращения: короткие щелчки, как трещотка барабана
    function startRumble() {
        var ctx = ensureAudio();
        if (!ctx || !soundEnabled) { return; }
        stopRumble();
        var osc = ctx.createOscillator();
        var gain = ctx.createGain();
        var lfo = ctx.createOscillator();
        var lfoGain = ctx.createGain();
        osc.type = 'sawtooth';
        osc.frequency.value = 55;
        lfo.type = 'square';
        lfo.frequency.value = 14;
        lfoGain.gain.value = 0.05;
        gain.gain.value = 0.05;
        lfo.connect(lfoGain).connect(gain.gain);
        osc.connect(gain).connect(ctx.destination);
        osc.start();
        lfo.start();
        spinOscillators = [osc, lfo];
    }

    function stopRumble() {
        spinOscillators.forEach(function (o) {
            try { o.stop(); } catch (e) { /* уже остановлен */ }
        });
        spinOscillators = [];
    }

    // «дзынь» остановки барабана — нота пентатоники, каждый следующий выше
    function stopChime(index) {
        beep([520, 660, 880][index] || 660, 0.18, 'triangle', 0.12);
        beep([524, 662, 884][index] || 664, 0.1, 'sine', 0.06, 0.02);
    }

    function winFanfare() {
        [523, 659, 784, 1046].forEach(function (freq, i) {
            beep(freq, 0.22, 'triangle', 0.11, i * 0.13);
        });
    }

    function loseNote() {
        beep(330, 0.25, 'sine', 0.09);
        beep(262, 0.3, 'sine', 0.09, 0.18);
    }

    // ---------- Управление звуком ----------

    function renderSoundToggle() {
        if (!soundToggle) { return; }
        soundToggle.textContent = soundEnabled ? '🔊 Звук вкл.' : '🔇 Звук выкл.';
        soundToggle.setAttribute('aria-pressed', String(soundEnabled));
    }

    if (soundToggle) {
        renderSoundToggle();
        soundToggle.addEventListener('click', function () {
            soundEnabled = !soundEnabled;
            try { localStorage.setItem('slots-sound', soundEnabled ? 'on' : 'off'); } catch (e) { /* ignore */ }
            if (!soundEnabled) { stopRumble(); }
            renderSoundToggle();
            if (soundEnabled) { beep(660, 0.08, 'sine', 0.08); }
        });
    }

    // ---------- Барабаны ----------

    function setSymbol(reel, symbol) {
        reel.querySelector('.symbol').textContent = symbol;
    }

    function spinReel(reel) {
        reel.classList.add('spinning');
        if (reducedMotion) { return null; }
        var timer = setInterval(function () {
            setSymbol(reel, SYMBOLS[Math.floor(Math.random() * SYMBOLS.length)]);
        }, TICK_MS);
        return timer;
    }

    function stopReel(reel, symbol, index) {
        reel.classList.remove('spinning');
        reel.classList.add('landed');
        setSymbol(reel, symbol);
        stopChime(index);
    }

    function resetReels() {
        reels.forEach(function (reel) {
            reel.classList.remove('spinning', 'landed');
            setSymbol(reel, '❔');
        });
    }

    function showResult(data) {
        var title = document.createElement('p');
        var text = document.createElement('p');
        title.className = 'title';
        text.className = 'text';

        if (data.isDemo) {
            // фан-режим администратора: результат не сохраняется
            title.textContent = data.symbols.join(' ') + ' — ' + (data.discountPercent > 0 ? 'выпало ' + data.discountPercent + '%' : 'без скидки');
            text.textContent = 'Фан-режим: прокрутка не сохранена, бонус не начисляется. Крутите ещё!';
        } else if (data.discountPercent > 0) {
            title.classList.add('win');
            title.textContent = '🎉 ' + data.symbols.join(' ') + ' — скидка ' + data.discountPercent + '%!';
            text.textContent = 'Бонус сохранён в аккаунте на 24 часа и применится к одному заказу при оформлении.';
            winFanfare();
        } else {
            title.textContent = data.symbols.join(' ') + ' — без скидки';
            text.textContent = 'Ничего не потеряно: попробуйте снова завтра.';
            loseNote();
        }

        resultPanel.innerHTML = '';
        resultPanel.appendChild(title);
        resultPanel.appendChild(text);
    }

    function showError(message) {
        var title = document.createElement('p');
        var text = document.createElement('p');
        title.className = 'title';
        title.textContent = 'Не вышло';
        text.className = 'text';
        text.textContent = message || 'Сервер недоступен. Попробуйте ещё раз.';
        resultPanel.innerHTML = '';
        resultPanel.appendChild(title);
        resultPanel.appendChild(text);
    }

    // ---------- Прокрутка ----------

    var busy = false;

    function spin() {
        if (busy) { return; }
        busy = true;
        button.disabled = true;
        button.textContent = 'Крутится…';
        resetReels();
        resultPanel.innerHTML = '<p class="text">Барабаны вращаются…</p>';

        var timers = reels.map(spinReel);
        var spinDuration = reducedMotion ? 400 : REEL_SPIN_MS;
        startRumble();

        var stopped = false;
        var request = fetch(SPIN_URL, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'same-origin'
        }).then(function (response) {
            return response.json().then(function (body) { return { ok: response.ok, body: body }; });
        });

        function finish(data) {
            stopped = true;
            timers.forEach(clearInterval);
            // барабаны останавливаются по очереди, слева направо
            data.symbols.forEach(function (symbol, i) {
                setTimeout(function () {
                    if (i === data.symbols.length - 1) { stopRumble(); }
                    stopReel(reels[i], symbol, i);
                }, i * (reducedMotion ? 150 : REEL_STAGGER_MS));
            });

            var doneDelay = reducedMotion
                ? 150 * data.symbols.length + 100
                : REEL_STAGGER_MS * (data.symbols.length - 1) + 250;
            setTimeout(function () {
                busy = false;
                if (data.isDemo) {
                    // фан-режим: кнопка остаётся активной для следующих прокруток
                    button.disabled = false;
                    button.textContent = SPIN_LABEL;
                } else {
                    button.disabled = false;
                    button.textContent = 'Сегодня уже крутили';
                    button.disabled = true;
                    button.setAttribute('aria-disabled', 'true');
                }
                showResult(data);
            }, doneDelay);
        }

        function fail(message) {
            stopped = true;
            timers.forEach(clearInterval);
            stopRumble();
            resetReels();
            busy = false;
            button.disabled = false;
            button.textContent = SPIN_LABEL;
            showError(message);
        }

        // страховка: если сеть зависла, не крутим вечно
        var guard = setTimeout(function () {
            if (!stopped) { fail('Сервер не ответил. Проверьте соединение и попробуйте ещё раз.'); }
        }, spinDuration + 8000);

        request.then(function (result) {
            clearTimeout(guard);
            if (!stopped) {
                if (result.ok) {
                    // даём барабанам докрутиться до spinDuration, потом останавливаем
                    setTimeout(function () { finish(result.body); }, Math.max(0, spinDuration - 0));
                } else {
                    fail(result.body && result.body.message);
                }
            }
        }).catch(function () {
            clearTimeout(guard);
            if (!stopped) { fail('Сервер недоступен. Попробуйте ещё раз.'); }
        });
    }

    if (button) {
        button.addEventListener('click', spin);
    }
})();
