// noinspection JSUnusedGlobalSymbols
function loadJs(sourceUrl, shouldDefer) {
    if (sourceUrl.Length === 0) {
        console.error("Invalid source URL");
        return;
    }

    let tag = document.createElement('script');
    tag.async = shouldDefer;
    tag.src = sourceUrl;
    tag.type = "text/javascript";

    tag.onload = function () {
        console.log("Script loaded successfully");
    }

    tag.onerror = function () {
        console.error("Failed to load script");
    }

    document.head.appendChild(tag);
}