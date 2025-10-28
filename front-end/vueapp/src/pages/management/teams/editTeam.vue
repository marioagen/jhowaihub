<template>
    <TeamsForm 
        :isEdit="true"
        :id="routeId"
    />
</template>

<script>
    import TeamsForm from '@/components/management/teams/TeamsForm.vue';
    export default {
        name: "EditTeam",
        components: {
            TeamsForm
        },
        computed: {
            routeId() {
                return parseInt(this.$route.params.id);
            },
        },
    }
</script>