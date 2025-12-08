<template>
    <UsersForm 
        :isEdit="true"
        :email="$route.params.email"
    />
</template>

<script>
    import UsersForm from '@/components/management/users/UsersForm.vue';
    export default {
        name: "EditQuizz",
        components: {
            UsersForm
        },
    }
</script>