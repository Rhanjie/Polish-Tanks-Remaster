(function () {
  var ITCH_URL = "";
  var GAME_WIDTH = 1280;
  var GAME_HEIGHT = 760;

  var itch = document.getElementById("itch-link");
  if (ITCH_URL) {
    itch.href = ITCH_URL;
  } else {
    itch.textContent = "itch.io page coming soon";
    itch.setAttribute("aria-disabled", "true");
    itch.removeAttribute("href");
  }

  var frame = document.getElementById("player");
  var play = document.getElementById("play");
  var playNote = document.getElementById("play-note");
  var buildInfo = document.getElementById("build-info");
  var releaseNotes = document.getElementById("release-notes");

  function fit(iframe) {
    var scale = frame.clientWidth / GAME_WIDTH;
    iframe.style.transform = "scale(" + scale + ")";
  }

  play.addEventListener("click", function () {
    var iframe = document.createElement("iframe");
    iframe.src = "play/index.html";
    iframe.title = "Polish Tanks Remaster";
    iframe.width = GAME_WIDTH;
    iframe.height = GAME_HEIGHT;
    iframe.allow = "fullscreen; autoplay";
    iframe.allowFullscreen = true;
    frame.style.aspectRatio = GAME_WIDTH + " / " + GAME_HEIGHT;
    frame.textContent = "";
    frame.appendChild(iframe);
    fit(iframe);
    window.addEventListener("resize", function () { fit(iframe); });
    iframe.focus();
  });

  fetch("build.json", { cache: "no-store" })
    .then(function (response) {
      if (!response.ok) throw new Error(response.status);
      return response.json();
    })
    .then(function (build) {
      var commit = build.sha ? build.sha.slice(0, 7) : "unknown";
      buildInfo.textContent = "Build " + build.label + " · version " + build.version +
        " · commit " + commit + " · published " + build.date;
      if (build.sizeMb) playNote.textContent = "About " + build.sizeMb + " MB to download. Desktop browser, keyboard and mouse.";
      if (build.release) {
        releaseNotes.href = build.release;
        releaseNotes.hidden = false;
      }
    })
    .catch(function () {
      buildInfo.textContent = "No web build is published yet.";
      play.disabled = true;
      play.setAttribute("aria-disabled", "true");
    });
})();
