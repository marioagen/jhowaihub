<template>
    <UsersForm 
        :isEdit="true"
        :id="routeId"
    />
</template>

<script>
    import UsersForm from '@/components/management/users/UsersForm.vue';
    export default {
        name: "EditQuizz",
        components: {
            UsersForm
        },
        computed: {
            routeId() {
                return parseInt(this.$route.params.id);
            },
        },
    }
</script>