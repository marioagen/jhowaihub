<template>
    <main class="settings-page">
        <div class="container-fluid scroll-area settings-page__container">
            <div class="row g-0">
                <div class="col-12">
                    <h5 class="mb-0 fw-bold settings-page__title">{{ $t("settings.title") }}</h5>
                    <p class="text-muted settings-page__subtitle">{{ $t("settings.subtitle") }}</p>

                    <TabsComponent
                        :tabs="tabsList"
                        color="custom"
                        ref="TabsComponent"
                        @selected="onTabSelected"
                    >
                        <template #llm-models>
                            <div class="main-div settings-page__panel">
                                <LlmModelsSettings />
                            </div>
                        </template>
                        <template #keys>
                            <div class="main-div settings-page__panel">
                                <ApiKeysSettings />
                            </div>
                        </template>
                        <template #global-variables>
                            <div class="main-div settings-page__panel">
                                <GlobalVariablesSettings />
                            </div>
                        </template>
                        <template #document-types>
                            <div class="main-div settings-page__panel">
                                <DocumentTypesSettings />
                            </div>
                        </template>
                    </TabsComponent>
                </div>
            </div>
        </div>
    </main>
</template>

<script>
    import TabsComponent from "@/components/global/TabsComponent.vue";
    import ApiKeysSettings from "@/components/settings/ApiKeysSettings.vue";
    import GlobalVariablesSettings from "@/components/settings/GlobalVariablesSettings.vue";
    import DocumentTypesSettings from "@/components/settings/DocumentTypesSettings.vue";
    import LlmModelsSettings from "@/components/settings/LlmModelsSettings.vue";

    export default {
        name: "SettingsIndex",
        components: {
            TabsComponent,
            LlmModelsSettings,
            ApiKeysSettings,
            GlobalVariablesSettings,
            DocumentTypesSettings,
        },
        data: () => ({
            tabsList: [
                {
                    name: "llm-models",
                    labelKey: "settings.tabs.llmModels",
                    icon: "BrainCircuit",
                },
                {
                    name: "keys",
                    labelKey: "settings.tabs.keys",
                    icon: "KeyRound",
                },
                {
                    name: "global-variables",
                    labelKey: "settings.tabs.globalVariables",
                    icon: "Braces",
                },
                {
                    name: "document-types",
                    labelKey: "settings.tabs.documentTypes",
                    icon: "Tags",
                },
            ],
        }),
        mounted() {
            this.applyRouteTab();
        },
        watch: {
            "$route.query.tab"() {
                this.applyRouteTab();
            },
        },
        methods: {
            applyRouteTab() {
                const activeTab = this.$route.query.tab;
                const validTab = this.tabsList.some((tab) => tab.name === activeTab);
                this.$refs.TabsComponent?.setActiveTab(validTab ? activeTab : this.tabsList[0].name);
            },
            onTabSelected(tabName) {
                if (this.$route.query.tab === tabName) return;
                this.$router.replace({ query: { tab: tabName } });
            },
        },
    };
</script>

<style scoped>
    .settings-page {
        width: 100%;
    }

    .settings-page__container {
        width: 100%;
        max-width: none;
        padding-right: 1rem;
    }

    .scroll-area {
        display: block;
        overflow-y: auto;
        width: 100%;
    }

    .settings-page__title {
        color: var(--color-heading-title, var(--color-body-content));
    }

    .settings-page__subtitle {
        margin-bottom: 0.75rem;
    }

    .settings-page__panel {
        width: 100%;
    }
</style>
