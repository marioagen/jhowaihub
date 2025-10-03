<template>
    <main>
        <div class="container-fluid scroll-area manage-user mx-2">
            <div class="row">
                <div class="col-12">
                    <h5 class="mb-0 fw-bold">{{ $t("labelUserManagement") }}</h5>
                    <p>{{ $t("labelUserManagementDescription") }}</p>
                    <TabsComponent :tabs="tabsList" color="custom">
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
                { name: "users", label: "labelUsers", icon: "UsersRound" },
                { name: "teams", label: "labelTeams", icon: "Building" },
                { name: "profiles", label: "labelProfilePermissions", icon: "Shield" },
            ],
        }),
    };
</script>

<style>
    .scroll-area {
        display: list-item;
        overflow-y: auto;
    }
</style>
