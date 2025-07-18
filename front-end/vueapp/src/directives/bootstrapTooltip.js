export default {
    mounted(el, binding) {
        if (typeof binding.value === 'string') {
            el.setAttribute('title', binding.value);
        }

        el._tooltipInstance = new window.bootstrap.Tooltip(el);
    },

    updated(el, binding) {
        if (typeof binding.value === 'string') {
            el.setAttribute('title', binding.value);
            el._tooltipInstance?.dispose();
            el._tooltipInstance = new window.bootstrap.Tooltip(el);
        }
    },

    unmounted(el) {
        el._tooltipInstance?.dispose();
    }
};