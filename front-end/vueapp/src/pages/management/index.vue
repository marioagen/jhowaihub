<template>
    <main>
        <div class="container-fluid scroll-area manage-user mx-2">
            <div class="row">
                <div class="col-12">
                    <h5 class="mb-0 fw-bold">{{ $t("management.title") }}</h5>
                    <p>{{ $t("management.subtitle") }}</p>
                    <TabsComponent
                        :tabs="tabsList"
                        color="custom"
                        ref="TabsComponent"
                    >
                        <template #users>
                            <UsersComponent />
                        </template>
                        <template #teams>
                            <TeamsComponent />
                        </template>
                        <template #profiles>
                            <ProfilesComponent />
                        </template>
                    </TabsComponent>
                </div>
            </div>
        </div>
    </main>
</template>

<script>
    import TabsComponent from "@/components/global/TabsComponent.vue";
    import TeamsComponent from "@/components/management/teams/TeamsComponent.vue";
    import UsersComponent from "@/components/management/users/UsersComponent.vue";
    import ProfilesComponent from "@/components/management/profiles/ProfilesComponent.vue";
    import { hasPermission } from "@/utils/permissions.js";

    export default {
        name: "ManagementIndex",
        components: {
            TabsComponent,
            TeamsComponent,
            UsersComponent,
            ProfilesComponent,
        },
        data: () => ({
            allTabs: [
                {
                    name: "users",
                    label: "management.users.title",
                    icon: "UsersRound",
                    module: "Management",
                    action: "Users",
                },
                {
                    name: "teams",
                    label: "management.teams.title",
                    icon: "Building",
                    module: "Management",
                    action: "Teams",
                },
                {
                    name: "profiles",
                    label: "management.profiles.title",
                    icon: "Shield",
                    module: "Management",
                    action: "Profiles",
                },
            ],
            tabsList: [],
        }),
        methods: {
            filterTabsByPermissions() {
                this.tabsList = this.allTabs.filter((tab) => hasPermission(tab.module, tab.action));
            },
        },
        mounted() {
            this.filterTabsByPermissions();

            if (this.tabsList.length === 0) {
                this.$router.push({ name: "home" });
                return;
            }

            let activeTab = this.$route.query.tab;

            const hasAccessToTab = this.tabsList.some((tab) => tab.name === activeTab);

            if (activeTab !== undefined && hasAccessToTab) {
                this.$refs.TabsComponent.setActiveTab(activeTab);
            } else {
                this.$refs.TabsComponent.setActiveTab(this.tabsList[0].name);
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
