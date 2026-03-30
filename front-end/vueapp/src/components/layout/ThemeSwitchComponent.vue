<template>
    <button
        class="btn btn-outline-primary table-btn btn-sm"
        type="button"
        @click="toggleTheme"
        aria-expanded="false"
        style="display: flex; align-items: center; justify-content: center"
    >
        <LucideIcon
            icon="Moon"
            v-if="isDarkMode"
        />
        <LucideIcon
            icon="Sun"
            v-else
        />
    </button>
</template>
<script>
    export default {
        name: "ThemeSwitchComponent",
        data() {
            return {
                currentTheme: localStorage.getItem("theme") || "css-theme-light",
            };
        },
        computed: {
            isDarkMode() {
                return this.currentTheme === "css-theme-dark";
            },
        },
        mounted() {
            const savedTheme = localStorage.getItem("theme");
            this.currentTheme =
                savedTheme === "css-theme-dark" ? "css-theme-dark" : "css-theme-light";
            this.setTheme(this.currentTheme);
        },
        methods: {
            toggleTheme() {
                if (localStorage.getItem("theme") === "css-theme-dark") {
                    this.setTheme("css-theme-light");
                } else {
                    this.setTheme("css-theme-dark");
                }
            },
            setTheme(themeName) {
                localStorage.setItem("theme", themeName);
                document.documentElement.className = themeName;
                this.currentTheme = themeName;
                this.$store.commit("setTheme", themeName);
            },
        },
    };
</script>
