<template>
    <main>
        <div
            class="container-fluid scroll-area manage-user mx-2"
        >
            <div class="row">
                <div class="col-12">
                    <h5 class="mb-0 fw-bold">
                        {{ $t("management.title") }}
                    </h5>
                    <p>{{ $t("management.subtitle") }}</p>
                    <TabsComponent
                        :tabs="tabsList"
                        color="custom"
                        ref="TabsComponent"
                    >
                        <template #quizzes>
                            <QuizzesPage />
                        </template>
                        <template #questions>
                            <QuestionsPage />
                        </template>
                        <template #types>
                            <TypePage />
                        </template>
                    </TabsComponent>
                </div>
            </div>
        </div>
    </main>
</template>
<script>
    import TabsComponent from "@/components/global/TabsComponent.vue";
    import QuestionsPage from "@/pages/managementQuizzes/questions/questions.vue";
    import QuizzesPage from "@/pages/managementQuizzes/quizzes/index.vue";
    import TypePage from "@/pages/managementQuizzes/types/types.vue";

    export default {
        name: "ManagementQuizzesPage",
        components: {
            TabsComponent,
            QuizzesPage,
            QuestionsPage,
            TypePage,
        },
        data: () => ({
            tabsList: [
                {
                    name: "quizzes",
                    label: "quizzes.title",
                    icon: "ClipboardList",
                },
                {
                    name: "questions",
                    label: "questions.title",
                    icon: "MessageSquare",
                },
                {
                    name: "types",
                    label: "types.title",
                    icon: "BookmarkCheck",
                },
            ],
        }),
        mounted() {
            let activeTab = this.$route.query.tab;
            if (activeTab !== undefined) {
                this.$refs.TabsComponent.setActiveTab(
                    activeTab
                );
            }
        },
    };
</script>
<style>
    .scroll-area {
        display: list-item;
        overflow-y: auto;
    }
</style>
