const version = import.meta.env.VITE_APP_VERSION;

if (version) {
    const key = "app_version";
    const stored = localStorage.getItem(key);
    if (stored && stored !== version) {
        localStorage.setItem(key, version);
        window.location.reload();
    } else {
        localStorage.setItem(key, version);
    }
}
