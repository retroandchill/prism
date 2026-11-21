; ModuleID = 'test-funcs'
source_filename = "test-funcs"
target datalayout = "e-m:w-p270:32:32-p271:32:32-p272:64:64-i64:64-i128:128-f80:128-n8:16:32:64-S128"
target triple = "x86_64-unknown-windows-msvc"

@test-funcs_V_N1a = local_unnamed_addr global i32 1
@test-funcs_V_N1b = external local_unnamed_addr global i32
@llvm.global_ctors = appending global [1 x { i32, ptr, ptr }] [{ i32, ptr, ptr } { i32 65535, ptr @"test-funcs_F_N12<GlobalCtor>E_R_V", ptr null }]

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define i32 @test-funcs_F_N3addE_I32_I32_R_I32(i32 %0, i32 %1) local_unnamed_addr #0 {
entry:
  %2 = add i32 %1, %0
  ret i32 %2
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define double @test-funcs_F_N3addE_F32_I32_R_F64(float %0, i32 %1) local_unnamed_addr #0 {
entry:
  %2 = fpext float %0 to double
  %3 = uitofp i32 %1 to double
  %4 = fadd double %2, %3
  ret double %4
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define i32 @test-funcs_F_N3maxE_I32_I32_R_I32(i32 %0, i32 %1) local_unnamed_addr #0 {
entry:
  %. = tail call i32 @llvm.smax.i32(i32 %0, i32 %1)
  ret i32 %.
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define i32 @test-funcs_F_N7if_trueE_B_I32_I32_R_I32(i8 %0, i32 %1, i32 %2) local_unnamed_addr #0 {
entry:
  %.not = icmp eq i8 %0, 0
  %. = select i1 %.not, i32 %2, i32 %1
  ret i32 %.
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define i32 @test-funcs_F_N10cast_checkE_I64_R_I32(i64 %0) local_unnamed_addr #0 {
entry:
  %1 = trunc i64 %0 to i32
  ret i32 %1
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define i32 @test-funcs_F_N3minE_I32_I32_R_I32(i32 %0, i32 %1) local_unnamed_addr #0 {
entry:
  %. = tail call i32 @llvm.smin.i32(i32 %0, i32 %1)
  ret i32 %.
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define i32 @test-funcs_F_N15compound_assignE_I32_I32_R_I32(i32 %0, i32 %1) local_unnamed_addr #0 {
entry:
  %2 = add i32 %1, %0
  ret i32 %2
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define range(i32 0, -2147483648) i32 @test-funcs_F_N10while_loopE_I32_R_I32(i32 %0) local_unnamed_addr #0 {
entry:
  %smax = tail call i32 @llvm.smax.i32(i32 %0, i32 0)
  ret i32 %smax
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define i32 @test-funcs_F_N18unconditional_loopE_I32_R_I32(i32 returned %0) local_unnamed_addr #0 {
entry:
  ret i32 %0
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define void @test-funcs_F_N8for_loopE_I32_R_V(i32 %0) local_unnamed_addr #0 {
entry:
  ret void
}

; Function Attrs: nofree norecurse nosync nounwind memory(none)
define void @test-funcs_F_N25nested_break_and_continueE_R_V() local_unnamed_addr #1 {
entry:
  %0 = alloca i8, align 1
  %1 = alloca i8, align 1
  br label %loop.head1.preheader

loop.head1.preheader:                             ; preds = %loop.head1.preheader.backedge, %entry
  %.02230 = phi i32 [ 0, %entry ], [ %.02230.be, %loop.head1.preheader.backedge ]
  %2 = icmp eq i32 %.02230, 5
  br i1 %2, label %eval.merge, label %eval.merge7.thread.9

eval.merge:                                       ; preds = %loop.head1.preheader
  store i1 false, ptr %0, align 1
  %.0..0..0..0.12.pr = load i8, ptr %0, align 1
  %.not = icmp eq i8 %.0..0..0..0.12.pr, 0
  br i1 %.not, label %eval.merge7, label %loop.increment10

eval.merge7:                                      ; preds = %eval.merge
  store i1 false, ptr %1, align 1
  %.0..0..0..0..pr = load i8, ptr %1, align 1
  %.not23 = icmp eq i8 %.0..0..0..0..pr, 0
  br i1 %.not23, label %eval.merge.1, label %loop.tail11

eval.merge.1:                                     ; preds = %eval.merge7
  store i1 false, ptr %0, align 1
  %.0..0..0..0.12.pr.1 = load i8, ptr %0, align 1
  %.not.1 = icmp eq i8 %.0..0..0..0.12.pr.1, 0
  br i1 %.not.1, label %eval.merge7.1, label %loop.increment10

eval.merge7.1:                                    ; preds = %eval.merge.1
  store i1 false, ptr %1, align 1
  %.0..0..0..0..pr.1 = load i8, ptr %1, align 1
  %.not23.1 = icmp eq i8 %.0..0..0..0..pr.1, 0
  br i1 %.not23.1, label %eval.merge.2, label %loop.tail11

eval.merge.2:                                     ; preds = %eval.merge7.1
  store i1 false, ptr %0, align 1
  %.0..0..0..0.12.pr.2 = load i8, ptr %0, align 1
  %.not.2 = icmp eq i8 %.0..0..0..0.12.pr.2, 0
  br i1 %.not.2, label %eval.merge7.2, label %loop.increment10

eval.merge7.2:                                    ; preds = %eval.merge.2
  store i1 false, ptr %1, align 1
  %.0..0..0..0..pr.2 = load i8, ptr %1, align 1
  %.not23.2 = icmp eq i8 %.0..0..0..0..pr.2, 0
  br i1 %.not23.2, label %eval.merge.3, label %loop.tail11

eval.merge.3:                                     ; preds = %eval.merge7.2
  store i1 false, ptr %0, align 1
  %.0..0..0..0.12.pr.3 = load i8, ptr %0, align 1
  %.not.3 = icmp eq i8 %.0..0..0..0.12.pr.3, 0
  br i1 %.not.3, label %eval.merge7.3, label %loop.increment10

eval.merge7.3:                                    ; preds = %eval.merge.3
  store i1 false, ptr %1, align 1
  %.0..0..0..0..pr.3 = load i8, ptr %1, align 1
  %.not23.3 = icmp eq i8 %.0..0..0..0..pr.3, 0
  br i1 %.not23.3, label %eval.merge.4, label %loop.tail11

eval.merge.4:                                     ; preds = %eval.merge7.3
  store i1 false, ptr %0, align 1
  %.0..0..0..0.12.pr.4 = load i8, ptr %0, align 1
  %.not.4 = icmp eq i8 %.0..0..0..0.12.pr.4, 0
  br i1 %.not.4, label %eval.merge7.4, label %loop.increment10

eval.merge7.4:                                    ; preds = %eval.merge.4
  store i1 false, ptr %1, align 1
  %.0..0..0..0..pr.4 = load i8, ptr %1, align 1
  %.not23.4 = icmp eq i8 %.0..0..0..0..pr.4, 0
  br i1 %.not23.4, label %eval.merge.5, label %loop.tail11

eval.merge.5:                                     ; preds = %eval.merge7.4
  store i1 true, ptr %0, align 1
  %.0..0..0..0.12.pr.5 = load i8, ptr %0, align 1
  %.not.5 = icmp eq i8 %.0..0..0..0.12.pr.5, 0
  br i1 %.not.5, label %eval.merge7.5, label %loop.increment10

eval.merge7.5:                                    ; preds = %eval.merge.5
  store i1 false, ptr %1, align 1
  %.0..0..0..0..pr.5 = load i8, ptr %1, align 1
  %.not23.5 = icmp eq i8 %.0..0..0..0..pr.5, 0
  br i1 %.not23.5, label %eval.merge.6, label %loop.tail11

eval.merge.6:                                     ; preds = %eval.merge7.5
  store i1 false, ptr %0, align 1
  %.0..0..0..0.12.pr.6 = load i8, ptr %0, align 1
  %.not.6 = icmp eq i8 %.0..0..0..0.12.pr.6, 0
  br i1 %.not.6, label %eval.merge7.6, label %loop.increment10

eval.merge7.6:                                    ; preds = %eval.merge.6
  store i1 false, ptr %1, align 1
  %.0..0..0..0..pr.6 = load i8, ptr %1, align 1
  %.not23.6 = icmp eq i8 %.0..0..0..0..pr.6, 0
  br i1 %.not23.6, label %eval.merge.7, label %loop.tail11

eval.merge.7:                                     ; preds = %eval.merge7.6
  store i1 false, ptr %0, align 1
  %.0..0..0..0.12.pr.7 = load i8, ptr %0, align 1
  %.not.7 = icmp eq i8 %.0..0..0..0.12.pr.7, 0
  br i1 %.not.7, label %eval.merge7.7, label %loop.increment10

eval.merge7.7:                                    ; preds = %eval.merge.7
  store i1 true, ptr %1, align 1
  %.0..0..0..0..pr.7 = load i8, ptr %1, align 1
  %.not23.7 = icmp eq i8 %.0..0..0..0..pr.7, 0
  br i1 %.not23.7, label %eval.merge.8, label %loop.tail11

eval.merge.8:                                     ; preds = %eval.merge7.7
  store i1 false, ptr %0, align 1
  %.0..0..0..0.12.pr.8 = load i8, ptr %0, align 1
  %.not.8 = icmp eq i8 %.0..0..0..0.12.pr.8, 0
  br i1 %.not.8, label %eval.merge7.8, label %loop.increment10

eval.merge7.8:                                    ; preds = %eval.merge.8
  store i1 false, ptr %1, align 1
  %.0..0..0..0..pr.8 = load i8, ptr %1, align 1
  %.not23.8 = icmp eq i8 %.0..0..0..0..pr.8, 0
  br i1 %.not23.8, label %eval.merge.9, label %loop.tail11

eval.merge7.thread.9:                             ; preds = %loop.head1.preheader
  store i8 0, ptr %0, align 1
  store i8 0, ptr %1, align 1
  br label %loop.increment10

eval.merge.9:                                     ; preds = %eval.merge7.8
  store i1 false, ptr %0, align 1
  %.0..0..0..0.12.pr.9 = load i8, ptr %0, align 1
  %.not.9 = icmp eq i8 %.0..0..0..0.12.pr.9, 0
  br i1 %.not.9, label %eval.merge7.9, label %loop.increment10

eval.merge7.9:                                    ; preds = %eval.merge.9
  store i1 false, ptr %1, align 1
  %.0..0..0..0..pr.9 = load i8, ptr %1, align 1
  %.not23.9.not = icmp eq i8 %.0..0..0..0..pr.9, 0
  %3 = add nuw nsw i32 %.02230, 1
  br i1 %.not23.9.not, label %loop.head1.preheader.backedge, label %loop.tail11

loop.increment10:                                 ; preds = %eval.merge7.thread.9, %eval.merge.9, %eval.merge.8, %eval.merge.7, %eval.merge.6, %eval.merge.5, %eval.merge.4, %eval.merge.3, %eval.merge.2, %eval.merge.1, %eval.merge
  %.old = add nuw nsw i32 %.02230, 1
  %exitcond.not.old = icmp eq i32 %.old, 10
  br i1 %exitcond.not.old, label %loop.tail11, label %loop.head1.preheader.backedge

loop.head1.preheader.backedge:                    ; preds = %loop.increment10, %eval.merge7.9
  %.02230.be = phi i32 [ %.old, %loop.increment10 ], [ %3, %eval.merge7.9 ]
  br label %loop.head1.preheader

loop.tail11:                                      ; preds = %loop.increment10, %eval.merge7, %eval.merge7.1, %eval.merge7.2, %eval.merge7.3, %eval.merge7.4, %eval.merge7.5, %eval.merge7.6, %eval.merge7.7, %eval.merge7.8, %eval.merge7.9
  ret void
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define void @test-funcs_F_N5printE_S__ref_R_V({ ptr, i64 } %0) local_unnamed_addr #0 {
entry:
  ret void
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(argmem: readwrite)
define void @test-funcs_F_N16add_by_referenceE_I32__mut__ref_I32_R_V(ptr nofree captures(none) %0, i32 %1) local_unnamed_addr #2 {
entry:
  %2 = load i32, ptr %0, align 4
  %3 = add i32 %2, %1
  store i32 %3, ptr %0, align 4
  ret void
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define i32 @test-funcs_F_N19act_on_by_referenceE_I32_I32_R_I32(i32 %0, i32 %1) local_unnamed_addr #0 {
entry:
  %2 = add i32 %1, %0
  ret i32 %2
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define void @test-funcs_F_N11print_maybeE_S__ref_I32_R_V({ ptr, i64 } %0, i32 %1) local_unnamed_addr #0 {
entry:
  ret void
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define i32 @test-funcs_F_N9get_indexE_I32__slice__ref_UZ_R_I32({ ptr, i64 } %0, i64 %1) local_unnamed_addr #0 {
entry:
  ret i32 undef
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define i32 @test-funcs_F_N9get_indexE_I32__arr_4_UZ_R_I32(ptr byval([4 x i32]) %0, i64 %1) local_unnamed_addr #0 {
entry:
  %2 = ptrtoint ptr %0 to i64
  %.sroa.0.0.extract.trunc = trunc i64 %2 to i32
  ret i32 %.sroa.0.0.extract.trunc
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define noundef i32 @test-funcs_F_N15get_second_itemE_I32__arr_4_R_I32(ptr nofree readnone byval([4 x i32]) captures(none) %0) local_unnamed_addr #0 {
entry:
  %slice = alloca [4 x i32], align 4
  %1 = ptrtoint ptr %slice to i64
  %.sroa.0.0.extract.trunc.i = trunc i64 %1 to i32
  ret i32 %.sroa.0.0.extract.trunc.i
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define noundef i32 @test-funcs_F_N22get_item_from_constantE_I32__arr_4_R_I32(ptr nofree readnone byval([4 x i32]) captures(none) %0) local_unnamed_addr #0 {
entry:
  %slice = alloca [4 x i32], align 4
  %1 = ptrtoint ptr %slice to i64
  %.sroa.0.0.extract.trunc.i = trunc i64 %1 to i32
  ret i32 %.sroa.0.0.extract.trunc.i
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define i32 @test-funcs_F_N26get_index_from_large_arrayE_I32__arr_5_R_I32(ptr nofree readnone byval([5 x i32]) captures(none) %0) local_unnamed_addr #0 {
entry:
  ret i32 undef
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define i32 @test-funcs_F_N35materialize_temporary_into_functionE_R_I32() local_unnamed_addr #0 {
entry:
  ret i32 undef
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define void @test-funcs_F_N17print_char_arraysE_R_V() local_unnamed_addr #0 {
entry:
  ret void
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define void @test-funcs_F_N16print_char_arrayE_C__slice__ref_R_V({ ptr, i64 } %0) local_unnamed_addr #0 {
entry:
  ret void
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define { i8, i32 } @test-funcs_F_N15nullable_checksE_B_R_I32__nullable(i8 %0) local_unnamed_addr #0 {
entry:
  %.not = icmp eq i8 %0, 0
  %1 = select i1 %.not, { i8, i32 } zeroinitializer, { i8, i32 } { i8 1, i32 3 }
  ret { i8, i32 } %1
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define i32 @test-funcs_F_N25nullable_arithmetic_checkE_I32__nullable_I32__nullable_R_I32({ i8, i32 } %0, { i8, i32 } %1) local_unnamed_addr #0 {
entry:
  %.fca.0.extract1 = extractvalue { i8, i32 } %0, 0
  %2 = alloca i8, align 1
  %3 = icmp eq i8 %.fca.0.extract1, 0
  br i1 %3, label %eval.merge, label %eval.merge.thread

eval.merge:                                       ; preds = %entry
  %.fca.0.extract = extractvalue { i8, i32 } %1, 0
  %4 = icmp eq i8 %.fca.0.extract, 0
  store i1 %4, ptr %2, align 1
  %.0..0..0..0..pre = load i8, ptr %2, align 1
  %.0..0..0..pre.fr = freeze i8 %.0..0..0..0..pre
  %5 = icmp eq i8 %.0..0..0..pre.fr, 0
  %.fca.1.extract = extractvalue { i8, i32 } %1, 1
  %.fca.1.extract2 = extractvalue { i8, i32 } %0, 1
  %6 = add i32 %.fca.1.extract, %.fca.1.extract2
  br i1 %5, label %eval.merge.thread, label %7

eval.merge.thread:                                ; preds = %entry, %eval.merge
  br label %7

7:                                                ; preds = %eval.merge, %eval.merge.thread
  %8 = phi i32 [ 0, %eval.merge.thread ], [ %6, %eval.merge ]
  ret i32 %8
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define void @test-funcs_F_N25handle_nullable_referenceE_I32__ref__nullable_R_V(ptr nofree readnone captures(none) %0) local_unnamed_addr #0 {
entry:
  ret void
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define void @test-funcs_F_N20handle_nullable_spanE_S__ref__nullable_R_V({ ptr, i64 } %0) local_unnamed_addr #0 {
entry:
  ret void
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(none)
define void @test-funcs_F_N13optional_boolE_B__nullable_R_V(i8 %0) local_unnamed_addr #0 {
entry:
  ret void
}

; Function Attrs: mustprogress nofree norecurse nosync nounwind willreturn memory(readwrite, argmem: none, inaccessiblemem: none, target_mem: none)
define void @"test-funcs_F_N12<GlobalCtor>E_R_V"() #3 {
entry:
  %0 = load i32, ptr @test-funcs_V_N1a, align 4
  %1 = add i32 %0, 2
  store i32 %1, ptr @test-funcs_V_N1b, align 4
  ret void
}

; Function Attrs: nocallback nocreateundeforpoison nofree nosync nounwind speculatable willreturn memory(none)
declare i32 @llvm.smax.i32(i32, i32) #4

; Function Attrs: nocallback nocreateundeforpoison nofree nosync nounwind speculatable willreturn memory(none)
declare i32 @llvm.smin.i32(i32, i32) #4

attributes #0 = { mustprogress nofree norecurse nosync nounwind willreturn memory(none) }
attributes #1 = { nofree norecurse nosync nounwind memory(none) }
attributes #2 = { mustprogress nofree norecurse nosync nounwind willreturn memory(argmem: readwrite) }
attributes #3 = { mustprogress nofree norecurse nosync nounwind willreturn memory(readwrite, argmem: none, inaccessiblemem: none, target_mem: none) }
attributes #4 = { nocallback nocreateundeforpoison nofree nosync nounwind speculatable willreturn memory(none) }
