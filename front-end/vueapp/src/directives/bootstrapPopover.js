export default {
    mounted(el, binding) {
        const placement = Object.keys(binding.modifiers)[0] || "top";
        const content = typeof binding.value === "string" ? binding.value : binding.value?.content ?? "";

        el._popoverInstance = new window.bootstrap.Popover(el, {
            content,
            placement,
            html: true,
            trigger: "click",
        });

        document.addEventListener("click", (e) => {
            if (!el.contains(e.target)) {
                el._popoverInstance?.hide();
            }
        });
    },

    updated(el, binding) {
        const placement = Object.keys(binding.modifiers)[0] || "top";
        const content = typeof binding.value === "string" ? binding.value : binding.value?.content ?? "";

        el._popoverInstance?.dispose();
        el._popoverInstance = new window.bootstrap.Popover(el, {
            content,
            placement,
            html: true,
            trigger: "click",
        });
    },

    unmounted(el) {
        el._popoverInstance?.dispose();
    },
};
