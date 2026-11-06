// Adds a "BetaSeries" entry to the user profile preferences menu.
(function () {
  var CLASS = 'lnkBetaSeriesPreferences';
  function inject() {
    if (document.querySelector('.' + CLASS)) { return; }
    var anchor = document.querySelector('.lnkDisplayPreferences');
    if (!anchor || !anchor.parentNode) { return; }
    var a = document.createElement('a');
    a.className = 'emby-button ' + CLASS + ' listItem-border';
    a.setAttribute('is', 'emby-linkbutton');
    a.style.cssText = 'display:block;margin:0;padding:0';
    a.href = '#/configurationpage?name=BetaSeriesUser';
    a.innerHTML = '<div class="listItem"><span class="material-icons listItemIcon listItemIcon-transparent sync" aria-hidden="true"></span>' +
      '<div class="listItemBody"><div class="listItemBodyText">BetaSeries</div></div></div>';
    anchor.parentNode.insertBefore(a, anchor.nextSibling);
  }
  new MutationObserver(inject).observe(document.body, { childList: true, subtree: true });
  inject();
})();
