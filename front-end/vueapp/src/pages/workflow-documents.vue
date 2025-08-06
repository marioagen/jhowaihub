<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            
        </div>
    </main>
</template>

<script>

    export default {
        name: "WorkflowDocumments",
        data() {
            return {
                crumbsData: [],
                entitySearch: {},
                resetInputSearch: false,
                modalQuestion: {
                    name: "",
                }
            };
        },
        components: { },
        watch: {
            "$store.state.userProfile.language": function () {
                this.setEntitySearch();
            },
        },
        methods: {}
    };
</script>

<style scoped>
</style>