<template>
    <div>
        <ul class="nav nav-pills nav-fill mt-3" role="tablist">
            <li
                v-for="(tab, index) in tabs"
                :key="index"
                class="nav-item"
                role="presentation"
            >
                <a
                    class="nav-link rounded-pill btn-custom"
                    :class="{ active: activeTab === tab.name }"
                    :id="`${tab.name}-tab`"
                    href="#"
                    role="tab"
                    @click.prevent="setActiveTab(tab.name)"
                    :aria-selected="activeTab === tab.name"
                    :aria-controls="tab.name"
                    type="button"
                >
                    <LucideIcon
                        v-if="tab.icon"
                        :icon="tab.icon"
                        size="16"
                        class="icon-pill"
                    />
                    {{ $t(tab.label) }}
                </a>
            </li>
        </ul>

        <div class="tab-content mt-3">
            <div
                v-if="activeTab"
                :id="activeTab"
                class="tab-pane active"
                role="tabpanel"
                :aria-labelledby="`${activeTab}-tab`"
            >
                <slot :name="activeTab" />
            </div>
        </div>
    </div>
</template>

<script>
    export default {
        name: "TabsComponent",
        props: {
            tabs: {
                type: Array,
                required: true,
                default: () => [],
            },
            color: {
                type: String,
                default: "light",
            },
        },
        data() {
            return {
                activeTab: this.tabs[0]?.name || "",
            };
        },
        methods: {
            setActiveTab(tabName) {
                this.activeTab = tabName;
                this.$emit("selected", tabName);
            },
        },
    };
</script>

<style scoped>
.nav-pills {
    background-color: var(--muted);
    border-radius: 50rem !important;
}
.nav-pills .nav-link.btn-custom {
    padding: 0.25rem 0.5rem;
    font-size: 0.775rem;
    font-weight: 500;
    color: #323338;
}
.nav-pills .nav-link.btn-custom.active {
    background-color: var(--color-card-content) !important;
    color: #323338;
}
.icon-pill {
    vertical-align: text-bottom;
    margin-right: 5px;
}
.tab-pane {
    padding: 0;
    background-color: transparent;
    border-radius: 0;
}
</style>
