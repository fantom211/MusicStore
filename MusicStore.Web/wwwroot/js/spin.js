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

    // ---------- Звуки игровых автоматов (оригинальные WAV-файлы) ----------
    var soundBase = '/sounds/slots/';
    var rumbleAudio = null;
    var activeOneShots = [];

    function playSound(file, volume, loop) {
        if (!soundEnabled) { return null; }
        var audio = new Audio(soundBase + file + '.wav');
        audio.preload = 'auto';
        audio.volume = volume == null ? 0.65 : volume;
        audio.loop = loop === true;
        activeOneShots.push(audio);
        audio.addEventListener('ended', function () {
            activeOneShots = activeOneShots.filter(function (item) { return item !== audio; });
        }, { once: true });
        var promise = audio.play();
        if (promise && promise.catch) {
            promise.catch(function () {
                activeOneShots = activeOneShots.filter(function (item) { return item !== audio; });
            });
        }
        return audio;
    }

    function startRumble() {
        stopRumble();
        rumbleAudio = playSound('slot-spin-loop', 0.34, true);
    }

    function stopRumble() {
        if (rumbleAudio) {
            rumbleAudio.pause();
            rumbleAudio.currentTime = 0;
            activeOneShots = activeOneShots.filter(function (item) { return item !== rumbleAudio; });
            rumbleAudio = null;
        }
    }

    function stopChime(index) {
        playSound('slot-reel-stop-' + (index + 1), 0.62, false);
    }

    function winFanfare() {
        playSound('slot-win', 0.72, false);
    }

    function loseNote() {
        playSound('slot-no-win', 0.52, false);
    }

    function stopAllSounds() {
        stopRumble();
        activeOneShots.forEach(function (audio) {
            audio.pause();
            audio.currentTime = 0;
        });
        activeOneShots = [];
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
            if (!soundEnabled) {
                stopAllSounds();
            }
            renderSoundToggle();
            if (soundEnabled) { playSound('slot-click', 0.55, false); }
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
            if (data.discountPercent > 0) { winFanfare(); } else { loseNote(); }
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
