<template>
    <main>
        <div class="container-fluid scroll-area manage-user mx-2">
            <div class="row">
                <div class="col-12">
                    <h4 class="mb-0">{{ $t('labelUserManagement') }}</h4>
                    <p>{{ $t('labelUserManagementDescription') }}</p>
                    <ul class="nav nav-pills nav-fill mt-3" role="tablist">
                        <li class="nav-item" role="presentation">
                            <a
                                class="nav-link rounded-pill btn-custom active"
                                id="user-tab"
                                data-bs-toggle="tab"
                                data-bs-target="#user"
                                type="button"
                                role="tab"
                                aria-controls="user"
                                aria-selected="true"
                            >
                                <img
                                    class="icon-pill"
                                    src="@/assets/img/users-tab.svg"
                                    width="16"
                                />
                                {{ $t('labelUsers') }}
                            </a>
                        </li>
                        <li class="nav-item" role="presentation">
                            <a
                                class="nav-link rounded-pill btn-custom"
                                id="team-tab"
                                data-bs-toggle="tab"
                                data-bs-target="#team"
                                type="button"
                                role="tab"
                                aria-controls="profile"
                                aria-selected="false"
                            >
                                <img
                                    class="icon-pill"
                                    src="@/assets/img/teams-tab.svg"
                                    width="16"
                                />
                                {{ $t('labelTeams') }}
                            </a>
                        </li>
                    </ul>
                    <div class="tab-content mt-3">
                        <div
                            class="tab-pane active"
                            id="user"
                            role="tabpanel"
                            aria-labelledby="user-tab"
                        >
                            <!-- Users -->
                            {{ $t('labelUsers') }}
                        </div>
                        <div class="tab-pane" id="team" role="tabpanel" aria-labelledby="team-tab">
                            <!-- Teams -->
                            <teams-manager />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>
<script>
    import TeamsManager from '@/components/pages/user/team.vue'

    export default {
        name: 'UserIndex',
        data() {
            return {
                // Data properties can be added here if needed
            }
        },
        components: {
            TeamsManager,
        },
        methods: {
            // Methods can be added here if needed
        },
    }
</script>
<style>
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
    .scroll-area {
        display: list-item;
        overflow-y: auto;
    }

    .manage-user {
        font-size: 0.775rem;
        color: #323338;
    }

    .icon-pill {
        vertical-align: text-bottom;
        margin-right: 5px;
    }
</style>
