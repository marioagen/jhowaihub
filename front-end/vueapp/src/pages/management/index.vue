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

    export default {
        name: "ManagementIndex",
        components: {
            TabsComponent,
            TeamsComponent,
            UsersComponent,
            ProfilesComponent,
        },
        data: () => ({
            tabsList: [
                { name: "users", label: "management.users.title", icon: "UsersRound" },
                { name: "teams", label: "management.teams.title", icon: "Building" },
                { name: "profiles", label: "management.profiles.title", icon: "Shield" },
            ],
        }),
        mounted() {
            let activeTab = this.$route.query.tab;
            if(activeTab !== undefined) {
                this.$refs.TabsComponent.setActiveTab(activeTab);
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
