<template>
    <div>
        <ApexChart
            :key="themeClass"
            type="bar"
            height="350"
            :options="chartOptions"
            :series="series"
        />
    </div>
</template>
<script>
    import ApexChart from "vue3-apexcharts";

    const DARK_THEME_CLASS = "css-theme-dark";

    const BAR_COLOR_DARK = "#60a5fa";

    function getThemeOverlay(isDark) {
        if (isDark) {
            return {
                colors: [BAR_COLOR_DARK],
                chart: { background: "transparent" },
                theme: { mode: "dark" },
                grid: { borderColor: "#393e5c", strokeDashArray: 4 },
                xaxis: {
                    labels: { style: { colors: "#9196b0" } },
                    axisBorder: { color: "#393e5c" },
                    axisTicks: { color: "#393e5c" },
                },
                yaxis: {
                    labels: { style: { colors: "#9196b0" } },
                    axisBorder: { show: true, color: "#393e5c" },
                },
                legend: { labels: { colors: "#b0b4c8" } },
                tooltip: { theme: "dark" },
            };
        }
        return {
            chart: { background: "transparent" },
            grid: { borderColor: "#e3e3e3" },
            xaxis: { labels: { style: { colors: "#373d3f" } } },
            yaxis: { labels: { style: { colors: "#373d3f" } } },
            tooltip: { theme: "light" },
        };
    }

    function deepMerge(base, overlay) {
        const result = { ...base };
        for (const key of Object.keys(overlay)) {
            if (
                overlay[key] != null &&
                typeof overlay[key] === "object" &&
                !Array.isArray(overlay[key]) &&
                base[key] != null &&
                typeof base[key] === "object" &&
                !Array.isArray(base[key])
            ) {
                result[key] = deepMerge(base[key], overlay[key]);
            } else {
                result[key] = overlay[key];
            }
        }
        return result;
    }

    export default {
        name: "BarGraphComponent",
        components: {
            ApexChart,
        },
        props: {
            series: {
                type: Object,
                required: true,
            },
            options: {
                type: Object,
                required: true,
            },
        },
        data() {
            return {
                themeClass:
                    typeof document !== "undefined" ? document.documentElement.className : "",
            };
        },
        computed: {
            isDarkMode() {
                return this.themeClass === DARK_THEME_CLASS;
            },
            chartOptions() {
                const overlay = getThemeOverlay(this.isDarkMode);
                return deepMerge({ ...this.options }, overlay);
            },
        },
        mounted() {
            if (typeof document === "undefined") return;
            this.themeClass = document.documentElement.className;
            this.observer = new MutationObserver(() => {
                this.themeClass = document.documentElement.className;
            });
            this.observer.observe(document.documentElement, {
                attributes: true,
                attributeFilter: ["class"],
            });
        },
        beforeUnmount() {
            if (this.observer) {
                this.observer.disconnect();
            }
        },
    };
</script>
