<template>
    <UsersForm 
        :isEdit="true"
        :id="$route.params.id"
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