<template>
    <div>
        <div v-if="tabBarCols">
            <div class="row">
                <div :class="`col-${tabBarCols}`">
                    <ul
                        class="nav nav-pills nav-fill mt-3"
                        role="tablist"
                        :class="{ 'nav-pills-compact': compact }"
                    >
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
                                    :size="compact ? 12 : 16"
                                    class="icon-pill"
                                />
                                {{ tabLabel(tab) }}
                            </a>
                        </li>
                    </ul>
                </div>
            </div>
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
        <template v-else>
            <ul
                class="nav nav-pills nav-fill mt-3"
                role="tablist"
                :class="{ 'nav-pills-compact': compact }"
            >
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
                            :size="compact ? 12 : 16"
                            class="icon-pill"
                        />
                        {{ tabLabel(tab) }}
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
        </template>
    </div>
</template>
<script>
    import { translateIfExists } from "@/utils/i18nHelpers";

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
            tabBarCols: {
                type: Number,
                default: null,
            },
            compact: {
                type: Boolean,
                default: false,
            },
        },
        data() {
            return {
                activeTab: this.tabs[0]?.name || "",
            };
        },
        methods: {
            tabLabel(tab) {
                const key = tab.labelKey || tab.label;
                return translateIfExists(this.$te, this.$t, key);
            },
            setActiveTab(tabName) {
                this.activeTab = tabName;
                this.$emit("selected", tabName);
            },
        },
    };
</script>
<style scoped>
    .nav-pills {
        background-color: var(--color-bg-nav-pills) !important;
        border-radius: 50rem !important;
        border-color: none;
        display: flex;
        align-items: stretch;
        padding: 3px;
        gap: 2px;
    }

    .nav-pills .nav-item {
        display: flex;
        align-items: stretch;
    }
    .nav-pills .nav-link.btn-custom {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        gap: 0.35rem;
        padding: 0 0.75rem;
        height: 32px;
        min-height: 32px;
        line-height: 1;
        font-size: 0.775rem;
        font-weight: 500;
        color: var(--color-body-content) !important;
        cursor: pointer;
        white-space: nowrap;
    }
    .nav-pills .nav-link.btn-custom.active {
        background-color: var(--color-bg-navbar) !important;
        color: var(--color-body-content) !important;
    }
    .icon-pill {
        display: inline-flex;
        align-items: center;
        flex-shrink: 0;
        margin-right: 0;
    }
    .nav-pills-compact .nav-link.btn-custom {
        padding: 0 0.5rem;
        height: 26px;
        min-height: 26px;
        font-size: 0.7rem;
        gap: 0.25rem;
    }
    .nav-pills-compact .icon-pill {
        margin-right: 0;
    }
    .tab-pane {
        padding: 0;
        background-color: transparent;
        border-radius: 0;
    }
</style>
