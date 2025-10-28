<template>
    <ProfilesForm 
        :isEdit="true"
        :id="routeId"
    />
</template>

<script>
    import ProfilesForm from '@/components/management/profiles/ProfilesForm.vue';
    export default {
        name: "EditProfile",
        components: {
            ProfilesForm
        },
        computed: {
            routeId() {
                return parseInt(this.$route.params.id);
            },
        },
    }
</script>